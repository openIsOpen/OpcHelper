using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OPCDAAUTO;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

        }
        [TestMethod]
        public void GetBranch()
        {
            List<string> leafs = new List<string>();
            OpcDAClientHelper client = new OpcDAClientHelper();
            client.Connect();
            client.GetUserNodeTags();
            List<string> branch = client.GetBranch("");
            for (int i = 0; i < branch.Count; i++)
            {
                if (!branch[i].StartsWith("_"))
                    leafs = client.GetBranch(branch[i]);
            }
            client.DisConnect();
        }
    }
}
