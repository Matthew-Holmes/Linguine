using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DataClasses;

namespace Linguine
{
    public partial class MainModel
    {
        public event EventHandler? SessionsChanged;

        public List<TextualMedia> OpenTextualMedia
        {
            get
            {
                using var context = ReadonlyLinguineFactory.CreateDbContext();
                return SM.Managers!.TextualMedia.GetOpen(context);
            }
        }

        internal bool OpenText(TextualMedia tm)
        {
            using var context = LinguineFactory.CreateDbContext();

            SM.Managers!.Sessions.Open(tm, context);

            return true;
        }


        internal void CloseText(TextualMedia tm)
        {

            using var context = LinguineFactory.CreateDbContext();

            SM.Managers!.Sessions.CloseText(tm, context);
        }
    }
}
