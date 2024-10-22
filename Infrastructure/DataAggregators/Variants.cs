using Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Variants : ManagerBase
    {
        public String Source { get; }


        public Variants(String source, String conn) : base(conn)
        {
            Source = source;
        }

        public IEnumerable<String> GetVariants(String root)
        {
            using LinguineContext lg = Linguine();
            return lg.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Root.Contains(root))
                .Select(v => v.Variant)
                .Distinct()
                .ToList();
        }

        public IEnumerable<String> GetRoots(String variant)
        {
            using LinguineContext lg = Linguine();
            return lg.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Variant.Contains(variant))
                .Select(v => v.Root)
                .Distinct()
                .ToList();
        }

        internal bool Add(VariantRoot variantRoot, LinguineContext? lg)
        {
            // TODO - test
            if (variantRoot.Source !=  Source)
            {
                return false;
            }

            if (lg is null)
            {
                using LinguineContext lg_tmp = Linguine();
                lg_tmp.Variants.Add(variantRoot);
                lg_tmp.SaveChanges();
            } else
            {
                lg.Variants.Add(variantRoot);
            }

            return true;
        }

        internal bool Add(List<VariantRoot> variantRoots)
        {
            // TODO - test
            if (variantRoots.Any(v => v.Source != Source))
            {
                return false;
            }

            using LinguineContext lg = Linguine();

            foreach (var vr in variantRoots)
            {
                Add(vr, lg);
            }

            lg.SaveChanges();

            return true;
        }

        internal bool DuplicateEntries()
        {
            // TODO - test
            using LinguineContext lg = Linguine();
            return lg.Variants
                      .Where(v => v.Source == Source)
                      .GroupBy(p => new { p.Variant, p.Root })
                      .Where(p => p.Count() > 1)
                      .Any();
        }
    }
}
