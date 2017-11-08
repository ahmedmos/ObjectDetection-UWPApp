using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace IntelligentKioskSample
{
    public class ChatTemplateModel
    {
        public Visibility isAdmin { get; set; }
        public Visibility isUser { get; set; }
        public string Comment { get; set; }
        public int TextColumn { get; set; }
        public TextAlignment Alignment { get; set; }
        public SolidColorBrush Backcolor { get; set; }
    }


}
