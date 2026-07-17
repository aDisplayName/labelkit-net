// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit.Internal;

using System.Text.RegularExpressions;

internal static class RegexMatchHelper
{
  internal static bool IsMatch(string input, string pattern, MatchingOptions options)
  {
    try
    {
      return Regex.IsMatch(input, pattern, options.RegexOptions, options.MatchTimeout);
    }
    catch (RegexMatchTimeoutException)
    {
      return false;
    }
    catch (ArgumentException)
    {
      return false;
    }
  }
}
