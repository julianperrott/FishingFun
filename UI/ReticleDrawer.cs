namespace FishingFun
{
    public class ReticleDrawer
    {
        public void Draw(System.Drawing.Bitmap bmp, System.Drawing.Point point)
        {
            var p = bmp.GetPixel(point.X, point.Y);

            bmp.SetPixel(point.X, point.Y, System.Drawing.Color.White);

            using (var gr = System.Drawing.Graphics.FromImage(bmp))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var thick_pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                {
                    var cornerSize = 15;
                    var recSize = 40;
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X - recSize, point.Y - recSize), cornerSize, cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X - recSize, point.Y + recSize), cornerSize, -cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X + recSize, point.Y - recSize), -cornerSize, cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X + recSize, point.Y + recSize), -cornerSize, -cornerSize);
                }
            }
        }

        private void DrawCorner(System.Drawing.Pen pen, System.Drawing.Graphics gr, System.Drawing.Point corner, int xDiff, int yDiff)
        {
            var lines = new System.Drawing.Point[]
            {
                new System.Drawing.Point(corner.X + xDiff, corner.Y),
                corner,
                new System.Drawing.Point(corner.X, corner.Y + yDiff)
            };

            gr.DrawLines(pen, lines);
        }
    }
}