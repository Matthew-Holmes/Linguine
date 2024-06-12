using Infrastructure;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine.Helpers
{
    internal class PageGenerator
    {
        private int _horizontalCharLimit;
        private int _verticalLineLimit;
        private TextualMedia _source;

        public PageGenerator(int horizontalCharLimit,
                             int verticalLineLimit, 
                             TextualMedia source)
        {
            _horizontalCharLimit = horizontalCharLimit;
            _verticalLineLimit = verticalLineLimit;
            _source = source;
        }

        String GeneratePage(int firstCharIndex, List<Statement> statements)
        {
            throw new NotImplementedException();
        }
    }
}
