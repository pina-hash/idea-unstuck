// Lists the COM types (interfaces, events, enums) embedded in an assembly
// built with EmbedInteropTypes, with their GUIDs, or the GUIDs of the same
// type names in an interop assembly. Runs on any OS; reads metadata only.
// Usage:
//   InteropCheck embedded <our.dll>             -> "FullName GUID" per embedded type
//   InteropCheck interop <interop.dll>...        -> identity line + "FullName GUID"
//   InteropCheck compare <old.dll> <new.dll> <interop.dll>...
//      Exit 0 only when every embedded type in both builds has the same GUID in
//      both, and every GUID also matches the interop assemblies named.
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

static class P
{
    static Dictionary<string, string> Types(string path, bool embeddedOnly, List<string> identity = null)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using var fs = File.OpenRead(path);
        using var pe = new PEReader(fs);
        var md = pe.GetMetadataReader();
        if (identity != null && md.IsAssembly)
        {
            var a = md.GetAssemblyDefinition();
            var key = md.GetBlobBytes(a.PublicKey);
            string token = "";
            if (key.Length > 0) { var h = System.Security.Cryptography.SHA1.HashData(key); token = string.Concat(h.Reverse().Take(8).Select(b => b.ToString("x2"))); }
            string company = "", product = "", fileVer = "";
            foreach (var ch in a.GetCustomAttributes())
            {
                var name = AttrName(md, md.GetCustomAttribute(ch));
                var val = FirstString(md, md.GetCustomAttribute(ch));
                if (name == "AssemblyCompanyAttribute") company = val;
                if (name == "AssemblyProductAttribute") product = val;
                if (name == "AssemblyFileVersionAttribute") fileVer = val;
            }
            identity.Add($"{md.GetString(a.Name)} {a.Version} token={token} file={fileVer} company={company} product={product}");
        }
        foreach (var th in md.TypeDefinitions)
        {
            var t = md.GetTypeDefinition(th);
            string guid = null, scope = null; bool typeId = false;
            foreach (var ch in t.GetCustomAttributes())
            {
                var ca = md.GetCustomAttribute(ch);
                var name = AttrName(md, ca);
                if (name == "GuidAttribute") guid = FirstString(md, ca);
                if (name == "TypeIdentifierAttribute") { typeId = true; var args = AllStrings(md, ca); if (args.Count == 2) scope = "scope:" + args[0].ToUpperInvariant(); }
            }
            if (embeddedOnly && !typeId) continue;
            guid ??= scope;
            if (guid == null) continue;
            var full = (md.GetString(t.Namespace) + "." + md.GetString(t.Name)).TrimStart('.');
            map[full] = guid.StartsWith("scope:") ? guid : guid.ToUpperInvariant();
        }
        return map;
    }
    // Method order of each embedded interface, including the _VtblGapN_M
    // placeholders the compiler writes. Same order = same vtable slots.
    static Dictionary<string, string> Layouts(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using var fs = File.OpenRead(path);
        using var pe = new PEReader(fs);
        var md = pe.GetMetadataReader();
        foreach (var th in md.TypeDefinitions)
        {
            var t = md.GetTypeDefinition(th);
            if ((t.Attributes & System.Reflection.TypeAttributes.Interface) == 0) continue;
            bool typeId = t.GetCustomAttributes().Any(ch => AttrName(md, md.GetCustomAttribute(ch)) == "TypeIdentifierAttribute");
            if (!typeId) continue;
            var full = md.GetString(t.Namespace) + "." + md.GetString(t.Name);
            map[full] = string.Join(",", t.GetMethods().Select(m => md.GetString(md.GetMethodDefinition(m).Name)));
        }
        return map;
    }
    // Slot number of every named method; _VtblGapN_M advances the slot by M.
    static Dictionary<string, int> Slots(string layout)
    {
        var r = new Dictionary<string, int>(); int slot = 0;
        foreach (var m in layout.Split(','))
        {
            var g = System.Text.RegularExpressions.Regex.Match(m, @"^_VtblGap\d+_(\d+)$");
            if (g.Success) { slot += int.Parse(g.Groups[1].Value); continue; }
            if (!r.ContainsKey(m)) r[m] = slot; slot++;
        }
        return r;
    }
    static string AttrName(MetadataReader md, CustomAttribute ca)
    {
        if (ca.Constructor.Kind == HandleKind.MemberReference)
        {
            var mr = md.GetMemberReference((MemberReferenceHandle)ca.Constructor);
            if (mr.Parent.Kind == HandleKind.TypeReference) return md.GetString(md.GetTypeReference((TypeReferenceHandle)mr.Parent).Name);
        }
        else if (ca.Constructor.Kind == HandleKind.MethodDefinition)
        {
            var m = md.GetMethodDefinition((MethodDefinitionHandle)ca.Constructor);
            return md.GetString(md.GetTypeDefinition(m.GetDeclaringType()).Name);
        }
        return "";
    }
    static List<string> AllStrings(MetadataReader md, CustomAttribute ca)
    {
        var r = new List<string>();
        var b = md.GetBlobReader(ca.Value);
        if (b.Length < 2 || b.ReadUInt16() != 1) return r;
        try { while (b.RemainingBytes > 2) { var s = b.ReadSerializedString(); if (s == null) break; r.Add(s); } } catch { }
        return r;
    }
    static string FirstString(MetadataReader md, CustomAttribute ca) => AllStrings(md, ca).FirstOrDefault() ?? "";
    static int Main(string[] a)
    {
        if (a.Length >= 2 && a[0] == "embedded") { foreach (var kv in Types(a[1], true).OrderBy(k => k.Key)) Console.WriteLine(kv.Key + " " + kv.Value); return 0; }
        if (a.Length >= 2 && a[0] == "interop")
        {
            foreach (var f in a.Skip(1)) { var id = new List<string>(); var t = Types(f, false, id); Console.WriteLine(string.Join("", id) + " types=" + t.Count); }
            return 0;
        }
        if (a.Length >= 4 && a[0] == "compare")
        {
            var oldT = Types(a[1], true); var newT = Types(a[2], true);
            var interop = new Dictionary<string, string>();
            foreach (var f in a.Skip(3)) foreach (var kv in Types(f, false)) interop[kv.Key] = kv.Value;
            int bad = 0, shared = 0;
            foreach (var name in oldT.Keys.Union(newT.Keys).OrderBy(k => k))
            {
                oldT.TryGetValue(name, out var o); newT.TryGetValue(name, out var n); interop.TryGetValue(name, out var i);
                string verdict;
                if (o != null && n != null) { shared++; verdict = o == n ? "SAME" : "DIFFERENT"; if (o != n) bad++; }
                else verdict = o == null ? "NEW-ONLY" : "OLD-ONLY";
                var g = n ?? o;
                if (i != null && i != g) { verdict += " INTEROP-MISMATCH(" + i + ")"; bad++; }
                if (i == null && !g.StartsWith("scope:")) { verdict += " NOT-IN-INTEROP"; bad++; }
                Console.WriteLine($"{verdict,-10} {name} old={o ?? "-"} new={n ?? "-"}");
            }
            var ol = Layouts(a[1]); var nl = Layouts(a[2]); int layoutSame = 0, layoutDiff = 0;
            foreach (var name in ol.Keys.Intersect(nl.Keys).OrderBy(k => k))
            {
                var os = Slots(ol[name]); var ns = Slots(nl[name]);
                var clash = os.Keys.Intersect(ns.Keys).Where(m => os[m] != ns[m]).ToList();
                if (clash.Count == 0) { layoutSame++; continue; }
                layoutDiff++; bad++;
                Console.WriteLine($"LAYOUT-DIFFERS {name}: " + string.Join(", ", clash.Select(m => $"{m} old slot {os[m]} new slot {ns[m]}")));
            }
            Console.WriteLine($"LAYOUT shared-interfaces={layoutSame + layoutDiff} same-slots={layoutSame} slot-conflicts={layoutDiff}");
            Console.WriteLine($"SUMMARY old={oldT.Count} new={newT.Count} shared={shared} problems={bad}");
            return bad == 0 && shared > 0 ? 0 : 1;
        }
        Console.Error.WriteLine("usage: embedded <dll> | interop <dll>... | compare <old> <new> <interop>...");
        return 2;
    }
}
