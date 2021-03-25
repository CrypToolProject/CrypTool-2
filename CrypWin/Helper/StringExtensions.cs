using System.Text.RegularExpressions;

namespace CrypTool.CrypWin.Helper
{
  public static class StringExtensions
  {
    public static bool IsValidEmailAddress(this string s) 
    { 
      Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"); 
      return regex.IsMatch(s); 
    }

    public static bool IsValidURL(this string s)
    {
      Regex regex = new Regex("(([a-zA-Z][0-9a-zA-Z+\\-\\.]*:)?/{0,2}[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?(#[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?");
      return regex.IsMatch(s);
    }
  }
}
