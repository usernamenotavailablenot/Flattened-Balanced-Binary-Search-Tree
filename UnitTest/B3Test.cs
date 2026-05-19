using B3Tree;
using System.Diagnostics;

namespace UnitTest
{
    [TestClass]
    public class B3Test
    {
        [TestMethod]
        public void TestB3Trees()
        {
            int N = 100000;
            int MIN = 128;
            int TRIAL = 10;
            int STEP = 1;
            Stopwatch sw = new Stopwatch();
            int[] insert = new int[N];
            int[] delete = new int[N];
            Dictionary<int, int>[] performance = new Dictionary<int, int>[TRIAL];
            for (int i = 0; i < TRIAL; ++i)
            {
                performance[i] = new Dictionary<int, int>();
            }
            for (int i = 0; i < N; ++i)
            {
                insert[i] = i;
                delete[i] = i;
            }
            for (int trial = 1; trial <= TRIAL; trial += 1) 
            {
                Shuffle(insert);
                Shuffle(delete);
                for (int min = 0; min <= MIN; min += STEP)
                {
                    B3Tree<int, int> b3 = new B3Tree<int, int>(min);
                    sw.Start();
                    TestB3Tree(b3, insert, delete, N);
                    sw.Stop();
                    performance[trial - 1].Add(min, (int)sw.ElapsedMilliseconds); 
                    sw.Reset();
                }
            }
            foreach (int min in performance[0].Keys.OrderBy(j => j))
            {
                Console.Write($"min = {(min).ToString("D3")}");
                for (int j = 0; j < TRIAL; ++j)
                {
                    int ms = performance[j][min];
                    Console.Write($" {ms}");
                }
                Console.WriteLine();
            }
        }
        private void TestB3Tree(B3Tree<int, int> b3, int[] insert, int[] delete, int N)
        {
            for (int i = 0; i < N; ++i)
            {
                int k = insert[i];
                b3.Add(k, k);
                bool b = b3.HasKey(k);
                Assert.IsTrue(b);
            }
            for (int i = 0; i < N; ++i)
            {
                int k = delete[i];
                b3.Remove(k);
                bool b = b3.HasKey(k);
                Assert.IsFalse(b);
            }
        }
        private static void Shuffle<T>(T[] arr)
        {
            Random r = new Random();
            for (int i = arr.Length - 1; i >= 0; --i)
            {
                int j = r.Next(i + 1);
                T tmp = arr[j];
                arr[j] = arr[i];
                arr[i] = tmp;
            }
        }
    }
}
