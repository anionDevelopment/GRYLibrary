using GRYLibrary.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Tests.Testcases.GraphTests
{
    //bookmark: ⭠⭢⭡⭣⭦⭧⭨⭩
    [TestClass]
    public class CycleFinderTest
    {
        [TestMethod]
        public void TestSimpleGraph()
        {
            /*
             * Graph:
             * A⭢B⭢C⭢D
             * ⭡ ⭣  ⭡  ⭣
             * H⭠G⭠F⭠E
             */
            DirectedGraph graph = new();
            Vertex a = new(nameof(a));
            graph.AddVertex(a);
            Vertex b = new(nameof(b));
            graph.AddVertex(b);
            Vertex c = new(nameof(c));
            graph.AddVertex(c);
            Vertex d = new(nameof(d));
            graph.AddVertex(d);
            Vertex e = new(nameof(e));
            graph.AddVertex(e);
            Vertex f = new(nameof(f));
            graph.AddVertex(f);
            Vertex g = new(nameof(g));
            graph.AddVertex(g);
            Vertex h = new(nameof(h));
            graph.AddVertex(h);

            DirectedEdge e01 = new(a, b, nameof(e01));
            graph.AddEdge(e01);
            DirectedEdge e02 = new(b, c, nameof(e02));
            graph.AddEdge(e02);
            DirectedEdge e03 = new(c, d, nameof(e03));
            graph.AddEdge(e03);
            DirectedEdge e04 = new(d, e, nameof(e04));
            graph.AddEdge(e04);
            DirectedEdge e05 = new(e, f, nameof(e05));
            graph.AddEdge(e05);
            DirectedEdge e06 = new(f, g, nameof(e06));
            graph.AddEdge(e06);
            DirectedEdge e07 = new(g, h, nameof(e07));
            graph.AddEdge(e07);
            DirectedEdge e08 = new(h, a, nameof(e08));
            graph.AddEdge(e08);
            DirectedEdge e09 = new(b, g, nameof(e09));
            graph.AddEdge(e09);
            DirectedEdge e10 = new(f, c, nameof(e10));
            graph.AddEdge(e10);

            ISet<Cycle> expectedCycles = new HashSet<Cycle>
            {
                new Cycle([e01, e02, e03, e04, e05, e06, e07, e08]),
                new Cycle([e01, e09, e07, e08]),
                new Cycle([e03, e04, e05, e10])
            };

            ISet<Cycle> foundCycles = graph.GetAllCycles();

            Assert.IsTrue(foundCycles.SetEquals(expectedCycles));
        }
        [TestMethod]
        public void TestSimpleGraph2()
        {
            DirectedGraph graph = new();
            Vertex a = new(nameof(a));
            graph.AddVertex(a);
            Vertex b = new(nameof(b));
            graph.AddVertex(b);
            Vertex c = new(nameof(c));
            graph.AddVertex(c);
            Vertex d = new(nameof(d));
            graph.AddVertex(d);
            Vertex e = new(nameof(e));
            graph.AddVertex(e);

            DirectedEdge e01 = new(a, b, nameof(e01));
            graph.AddEdge(e01);
            DirectedEdge e02 = new(b, c, nameof(e02));
            graph.AddEdge(e02);
            DirectedEdge e03 = new(c, d, nameof(e03));
            graph.AddEdge(e03);
            DirectedEdge e04 = new(d, e, nameof(e04));
            graph.AddEdge(e04);
            DirectedEdge e05 = new(e, a, nameof(e05));
            graph.AddEdge(e05);
            DirectedEdge e06 = new(a, c, nameof(e06));
            graph.AddEdge(e06);
            DirectedEdge e07 = new(b, e, nameof(e07));
            graph.AddEdge(e07);
            DirectedEdge e08 = new(d, b, nameof(e08));
            graph.AddEdge(e08);

            Assert.AreNotEqual(new Cycle([e01, e02, e03, e04, e05]), new Cycle([e06, e03, e08, e07, e05]));

            ISet<Cycle> expectedCycles = new HashSet<Cycle>
            {
                new Cycle([e01, e02, e03, e04, e05]),
                new Cycle([e06, e03, e08, e07, e05]),
                new Cycle([e02, e03, e08]),
                new Cycle([e01, e07, e05]),
                new Cycle([e06, e03, e04, e05])
            };

            ISet<Cycle> foundCycles = graph.GetAllCycles();

            Assert.IsTrue(foundCycles.SetEquals(expectedCycles), $"Expected <{Cycle.CycleSetToString(expectedCycles)}> but found <{Cycle.CycleSetToString(foundCycles)}>");

        }
        [TestMethod]
        public void TestSimpleGraph3()
        {
            DirectedGraph graph = new();
            Vertex a = new(nameof(a));
            graph.AddVertex(a);
            Vertex b = new(nameof(b));
            graph.AddVertex(b);
            Vertex c = new(nameof(c));
            graph.AddVertex(c);
            Vertex d = new(nameof(d));
            graph.AddVertex(d);
            Vertex e = new(nameof(e));
            graph.AddVertex(e);
            Vertex f = new(nameof(f));
            graph.AddVertex(f);
            Vertex g = new(nameof(g));
            graph.AddVertex(g);
            Vertex h = new(nameof(h));
            graph.AddVertex(h);
            Vertex i = new(nameof(i));
            graph.AddVertex(i);
            Vertex j = new(nameof(j));
            graph.AddVertex(j);
            Vertex k = new(nameof(k));
            graph.AddVertex(k);

            DirectedEdge e_ab = new(a, b, nameof(e_ab));
            graph.AddEdge(e_ab);
            DirectedEdge e_ga = new(g, a, nameof(e_ga));
            graph.AddEdge(e_ga);
            DirectedEdge e_ij = new(i, j, nameof(e_ij));
            graph.AddEdge(e_ij);
            DirectedEdge e_hi = new(h, i, nameof(e_hi));
            graph.AddEdge(e_hi);
            DirectedEdge e_jb = new(j, b, nameof(e_jb));
            graph.AddEdge(e_jb);
            DirectedEdge e_bc = new(b, c, nameof(e_bc));
            graph.AddEdge(e_bc);
            DirectedEdge e_jc = new(j, c, nameof(e_jc));
            graph.AddEdge(e_jc);
            DirectedEdge e_jk = new(j, k, nameof(e_jk));
            graph.AddEdge(e_jk);
            DirectedEdge e_cd = new(c, d, nameof(e_cd));
            graph.AddEdge(e_cd);
            DirectedEdge e_kd = new(k, d, nameof(e_kd));
            graph.AddEdge(e_kd);
            DirectedEdge e_de = new(d, e, nameof(e_de));
            graph.AddEdge(e_de);
            DirectedEdge e_df = new(d, f, nameof(e_df));
            graph.AddEdge(e_df);
            DirectedEdge e_dh = new(d, h, nameof(e_dh));
            graph.AddEdge(e_dh);
            DirectedEdge e_eg = new(e, g, nameof(e_eg));
            graph.AddEdge(e_eg);
            DirectedEdge e_fg = new(f, g, nameof(e_fg));
            graph.AddEdge(e_fg);

            ISet<Cycle> expectedCycles = new HashSet<Cycle>
            {
                new Cycle([e_ab, e_bc, e_cd, e_de, e_eg, e_ga]),
                new Cycle([e_ab, e_bc, e_cd, e_df, e_fg, e_ga]),
                new Cycle([e_hi, e_ij, e_jk, e_kd, e_dh]),
                new Cycle([e_hi, e_ij, e_jc, e_cd, e_dh]),
                new Cycle([e_hi, e_ij, e_jb, e_bc, e_cd, e_dh])
            };
            Assert.AreEqual(5, expectedCycles.Count);

            ISet<Cycle> foundCycles = graph.GetAllCycles();

            Assert.IsTrue(foundCycles.SetEquals(expectedCycles), $"Expected <{Cycle.CycleSetToString(expectedCycles)}> but found <{Cycle.CycleSetToString(foundCycles)}>");
        }
        [TestMethod]
        public void TestSimpleGraph4()
        {
            DirectedGraph graph = new();
            Vertex a = new(nameof(a));
            graph.AddVertex(a);
            Vertex b = new(nameof(b));
            graph.AddVertex(b);
            Vertex c = new(nameof(c));
            graph.AddVertex(c);

            DirectedEdge e_ab = new(a, b, nameof(e_ab));
            graph.AddEdge(e_ab);
            DirectedEdge e_bc = new(b, c, nameof(e_bc));
            graph.AddEdge(e_bc);
            DirectedEdge e_ca = new(c, a, nameof(e_ca));
            graph.AddEdge(e_ca);
            DirectedEdge e_ba = new(b, a, nameof(e_ba));
            graph.AddEdge(e_ba);
            DirectedEdge e_cb = new(c, b, nameof(e_cb));
            graph.AddEdge(e_cb);
            DirectedEdge e_ac = new(a, c, nameof(e_ac));
            graph.AddEdge(e_ac);

            ISet<Cycle> expectedCycles = new HashSet<Cycle>
            {
                new Cycle([e_ab, e_bc, e_ca]),
                new Cycle([e_ba, e_ac, e_cb]),
                new Cycle([e_ab, e_ba]),
                new Cycle([e_bc, e_cb]),
                new Cycle([e_ca, e_ac])
            };
            Assert.AreEqual(5, expectedCycles.Count);
            ISet<Cycle> foundCycles = graph.GetAllCycles();

            Assert.IsTrue(foundCycles.SetEquals(expectedCycles), $"Expected <{Cycle.CycleSetToString(expectedCycles)}> but found <{Cycle.CycleSetToString(foundCycles)}>");
        }
    }
}