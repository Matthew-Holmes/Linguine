using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public interface IDefinitionFrequencyEngine
    {
        public FrequencyData ComputeFrequencyData(LinguineReadonlyDbContext context);

    }
}
