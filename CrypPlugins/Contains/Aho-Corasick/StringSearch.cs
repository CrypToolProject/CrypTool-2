/*
   Copyright 2008 Thomas Schmid, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections;

namespace Contains.Aho_Corasick
{
  public class StringSearch : IStringSearchAlgorithm
  {
    private string[] _keywords;
    private TreeNode _root;
    // private string dicName;

    public StringSearch(string[] keywords)
    {
      // this.dicName = DicName;
      this.Keywords = keywords;
    }

    private void BuildTree()
    {
      // System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
      // sw.Start();
      // use AppDomain Cache to avoid overhead of regeneration of tree every time.
      //if (AppDomain.CurrentDomain.GetData(dicName) != null)
      //  this._root = (TreeNode)AppDomain.CurrentDomain.GetData(dicName);
      // else  // no tree in Cache, generate it and store in Cache
      // {
        this._root = new TreeNode(null, ' ');
        TreeNode node1 = null;
        foreach (string str in this._keywords)
        {
          node1 = this._root;
          foreach (char ch in str)
          {
            TreeNode node2 = null;
            foreach (TreeNode node3 in node1.Transitions)
            {
              if (node3.Char == ch)
              {
                node2 = node3;
                break;
              }
            }
            if (node2 == null)
            {
              node2 = new TreeNode(node1, ch);
              node1.AddTransition(node2);
            }
            node1 = node2;
          }
          node1.AddResult(str);
        }
        ArrayList list = new ArrayList();
        foreach (TreeNode node in this._root.Transitions)
        {
          node.Failure = this._root;
          foreach (TreeNode node3 in node.Transitions)
          {
            list.Add(node3);
          }
        }
        while (list.Count != 0)
        {
          ArrayList list2 = new ArrayList();
          Char ch;
          foreach (TreeNode node in list)
          {
            TreeNode failure = node.Parent.Failure;
            ch = node.Char;
            while ((failure != null) && !failure.ContainsTransition(ch))
            {
              failure = failure.Failure;
            }
            if (failure == null)
            {
              node.Failure = this._root;
            }
            else
            {
              node.Failure = failure.GetTransition(ch);
              foreach (string str2 in node.Failure.Results)
              {
                node.AddResult(str2);
              }
            }
            foreach (TreeNode node5 in node.Transitions)
            {
              list2.Add(node5);
            }
          }
          list = list2;
        }
        this._root.Failure = this._root;
        // System.AppDomain.CurrentDomain.SetData(dicName, this._root);
      //}
      // sw.Stop();
      // System.Diagnostics.Debug.WriteLine(sw.Elapsed.ToString() );
    }

    public bool ContainsAny(string text)
    {
      if (text != null)
      {
        TreeNode failure = this._root;
        for (int i = 0; i < text.Length; i++)
        {
          TreeNode transition = null;
          while (transition == null)
          {
            transition = failure.GetTransition(text[i]);
            if (failure == this._root)
            {
              break;
            }
            if (transition == null)
            {
              failure = failure.Failure;
            }
          }
          if (transition != null)
          {
            failure = transition;
          }
          if (failure.Results.Length > 0)
          {
            return true;
          }
        }
      }
      return false;
    }

    public StringSearchResult[] FindAll(string text)
    {
      if (text != null)
      {
        ArrayList list = new ArrayList();
        TreeNode failure = this._root;
        for (int i = 0; i < text.Length; i++)
        {
          TreeNode transition = null;
          while (transition == null)
          {
            transition = failure.GetTransition(text[i]);
            if (failure == this._root)
            {
              break;
            }
            if (transition == null)
            {
              failure = failure.Failure;
            }
          }
          if (transition != null)
          {
            failure = transition;
          }
          foreach (string str in failure.Results)
          {
            list.Add(new StringSearchResult((i - str.Length) + 1, str));
          }
        }
        return (StringSearchResult[])list.ToArray(typeof(StringSearchResult));
      }
      else
      {
        return new StringSearchResult[0];
      }
    }

    public StringSearchResult FindFirst(string text)
    {
      if (text != null)
      {
        ArrayList list = new ArrayList();
        TreeNode failure = this._root;
        for (int i = 0; i < text.Length; i++)
        {
          TreeNode transition = null;
          while (transition == null)
          {
            transition = failure.GetTransition(text[i]);
            if (failure == this._root)
            {
              break;
            }
            if (transition == null)
            {
              failure = failure.Failure;
            }
          }
          if (transition != null)
          {
            failure = transition;
          }
          string[] results = failure.Results;
          int index = 0;
          while (index < results.Length)
          {
            string keyword = results[index];
            return new StringSearchResult((i - keyword.Length) + 1, keyword);
          }
        }
      }
      return StringSearchResult.Empty;
    }

    public string[] Keywords
    {
      get
      {
        return this._keywords;
      }
      set
      {
        this._keywords = value;
        this.BuildTree();
      }
    }


    private class TreeNode
    {
      private char _char;
      private StringSearch.TreeNode _failure;
      private StringSearch.TreeNode _parent;
      private ArrayList _results;
      private string[] _resultsAr;
      private Hashtable _transHash;
      private StringSearch.TreeNode[] _transitionsAr;

      public TreeNode(StringSearch.TreeNode parent, char c)
      {
        this._char = c;
        this._parent = parent;
        this._results = new ArrayList();
        this._resultsAr = new string[0];
        this._transitionsAr = new StringSearch.TreeNode[0];
        this._transHash = new Hashtable();
      }

      public void AddResult(string result)
      {
        if (!this._results.Contains(result))
        {
          this._results.Add(result);
          this._resultsAr = (string[])this._results.ToArray(typeof(string));
        }
      }

      public void AddTransition(StringSearch.TreeNode node)
      {
        this._transHash.Add(node.Char, node);
        StringSearch.TreeNode[] array = new StringSearch.TreeNode[this._transHash.Values.Count];
        this._transHash.Values.CopyTo(array, 0);
        this._transitionsAr = array;
      }

      public bool ContainsTransition(char c)
      {
        return (this.GetTransition(c) != null);
      }

      public StringSearch.TreeNode GetTransition(char c)
      {
        return (StringSearch.TreeNode)this._transHash[c];
      }

      public char Char
      {
        get
        {
          return this._char;
        }
      }

      public StringSearch.TreeNode Failure
      {
        get
        {
          return this._failure;
        }
        set
        {
          this._failure = value;
        }
      }

      public StringSearch.TreeNode Parent
      {
        get
        {
          return this._parent;
        }
      }

      public string[] Results
      {
        get
        {
          return this._resultsAr;
        }
      }

      public StringSearch.TreeNode[] Transitions
      {
        get
        {
          return this._transitionsAr;
        }
      }
    }
  }

}
