using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

namespace PointsRotate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const float _sin = 0.5F;
        private const float _cos = 1.732F / 2;

        private Point[] points = null;
        private List<Result> resultList = null;

        private int pointsCount;
        private int threadCount;

        private DateTime totalBeginTime;
        private DateTime totalEndTime;

        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                this.pointsCount = (int)(float.Parse(txtPointsCount.Text) * 10000);

                if (points == null || points.Length != this.pointsCount)
                {
                    CreatePoints(this.pointsCount);
                }


                Rotate();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Rotate()
        {
            this.threadCount = int.Parse(txtThreadCount.Text);

            int length = this.pointsCount / threadCount;

            int beginIndex = 0;

            resultList = new List<Result>();
            this.totalBeginTime = DateTime.Now;

            for (int i = 0; i<threadCount; i++)
            {
                Para para = new Para();
                para.beginIndex = beginIndex;
                if (i == threadCount - 1)
                    para.endIndex = pointsCount - 1;
                else
                    para.endIndex = beginIndex + length - 1;

                beginIndex = para.endIndex + 1;

                Thread thread = new Thread(new ParameterizedThreadStart(Rotate));
                thread.Start(para);
            }
            
        }

        private void Rotate(object o)
        {
            Para para = (Para)o;

            DateTime beginTime = DateTime.Now;

            for (int i = para.beginIndex; i <= para.endIndex; i++)
            {
                Point p = points[i];

                // 围绕 z 轴 旋转
                p.x = p.x * _cos - p.y * _sin;
                p.y = p.y * _cos + p.x * _sin;

                // 围绕 y 轴 旋转
                p.x = p.x * _cos - p.z * _sin;
                p.z = p.z * _cos + p.x * _sin;

                // 围绕 x 轴 旋转
                p.y = p.y * _cos - p.z * _sin;
                p.z = p.z * _cos + p.y * _sin;

            }

            DateTime endTime = DateTime.Now;

            double t = (endTime - beginTime).TotalMilliseconds;

            lock (this.resultList)
            {
                this.resultList.Add(new Result(para.endIndex - para.beginIndex + 1, t));

                if (this.resultList.Count == this.threadCount)
                {
                    this.totalEndTime = endTime;
                    ShowResult();
                }
            }
        }

        private void ShowResult()
        {
            StringBuilder sb = new StringBuilder();

            int sumPointsCount = 0;

            for (int i = 0; i < this.resultList.Count; i++)
            {
                Result r = this.resultList[i];
                sb.Append("线程 " + ( i + 1 ) + " 执行三维旋转 " + r.pointsCount + " 个点， 耗时 ： " + r.t + " 毫秒\r\n");

                sumPointsCount += r.pointsCount;

            }

            double totalT = (this.totalEndTime - this.totalBeginTime).TotalMilliseconds;

            sb.Append("合计 ： " + this.resultList.Count + " 线程 执行三维旋转 共 " + sumPointsCount + " 个点， 耗时 ： " + totalT + " 毫秒， "
                + "每秒可执行 " + 1000D / totalT + " 次\r\n\r\n");


            WriteMsg(sb.ToString());

        }

        private void CreatePoints(int pointsCount)
        {
            lock (this)
            {
                points = new Point[pointsCount];

                for (int i=1; i<points.Length; i++)
                {
                    points[i].x = points[i - 1].x + 1;
                    points[i].y = points[i - 1].y + 1;
                    points[i].z = points[i - 1].z + 1;
                }
            }
        }

        private void WriteMsg(string msg)
        {
            this.BeginInvoke(new Action(() => { textBox1.AppendText(msg + "\r\n\r\n"); }));
        }

        private class Para
        {
            public int beginIndex;
            public int endIndex;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
        }
    }

    internal struct Point
    {
        public float x;
        public float y;
        public float z;
    }

    internal struct Result
    {
        public int pointsCount;
        public double t;

        public Result(int pointsCount, double t)
        {
            this.pointsCount = pointsCount;
            this.t = t;
        }
    }
}
