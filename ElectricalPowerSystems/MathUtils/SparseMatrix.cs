using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.MathUtils
{
    //самый простой вариант - список значений и индексов
    public class SparseMatrix<T>
    {
        public class Entry
        {
            public int I {  get; protected set; }
            public int J { get; protected set; }
            public T Value;
            public Entry(int i, int j, T value)
            {
                I = i;
                J = j;
                Value = value;
            }
        }
        int width;
        int height;
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        List<Entry> entries;
        private SparseMatrix(int w, int h)
        {
            this.width = w;
            this.height = h;
            this.entries = new List<Entry>();
        }
        public static SparseMatrix<T> Build(int w,int h)
        {
            SparseMatrix<T> result = new SparseMatrix<T>(w,h);
            return result;
        }
        public void Add(int i, int j, T value)
        {
            entries.Add(new Entry(i,j,value));
        }
        public Entry GetEntry(int i,int j)
        {
            foreach (var entry in entries)
            {
                if (entry.I == i && entry.J == j)
                {
                    return entry;
                }
            }
            throw new KeyNotFoundException();
        }
        public List<Entry> GetEntries()
        {
            return entries;
        }
    }
}
