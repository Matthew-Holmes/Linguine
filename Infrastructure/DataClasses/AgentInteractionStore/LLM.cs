using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    // Used in JSON so only add new models to the end
    public enum LLM
    {
        ChatGPT3_5,
        ChatGPT4o,
        ChatGPT4o_mini,
        DeepSeek_chat,
        Dummy,
    }
}
