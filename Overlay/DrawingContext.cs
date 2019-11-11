using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using Vector3 = SlimDX.Vector3;

namespace SliceIsRightTracker.Overlay
{
    public class DrawingContext : IDisposable
    {
        private VertexDeclaration _coloredVertexDecl;
        private ColoredVertex[] _vertexBuffer = new ColoredVertex[1000];
        private readonly int[] _indexBuffer = new int[1000];

        public DrawingContext(Device device)
        {
            Device = device;
        }

        public Device Device { get; }

        public void Dispose()
        {
            _coloredVertexDecl?.Dispose();
        }

        private void SetDeclaration()
        {
            if (_coloredVertexDecl == null)
                _coloredVertexDecl = ColoredVertex.GetDecl(Device);

            Device.VertexDeclaration = _coloredVertexDecl;
            Device.VertexFormat = ColoredVertex.Format;
        }

        public void DrawCircleWithPoint(Clio.Utilities.Vector3 center, float heading, float radius, Color color, Color pointColor)
        {
            int slices = 30;
            var radsPerSlice = (float)(Math.PI * 2 / slices);

            var newCenter = new Vector3(center.X, center.Y, center.Z);

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3(radius, 0, 0), color);

            for (int i = 0; i < slices; i++)
            {
                double h = ((Math.PI * 2) - heading) + (Math.PI / 2);
                if (h > (Math.PI * 2))
                    h = h - (Math.PI * 2);

                bool watchAt = (i * radsPerSlice) < h && h < ((i + 1) * radsPerSlice);

                var sine = (float)Math.Sin((i + 1) * radsPerSlice);
                var cosine = (float)Math.Cos((i + 1) * radsPerSlice);

                _vertexBuffer[2 + i] =
                    new ColoredVertex(newCenter + new Vector3(cosine * radius, 0, sine * radius),
                        watchAt ? pointColor.ToArgb() : color.ToArgb());
            }

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, slices, _vertexBuffer);
        }

        public void DrawAgroLine(Clio.Utilities.Vector3 center, float heading, float width, float height, Color color, Color pointColor)
        {
            var newCenter = new Vector3(center.X, center.Y, center.Z);

            float heightBack = width;

            float diag = (float)Math.Sqrt(height * height + width * width) / 2;
            float diagBack = (float)Math.Sqrt(heightBack * heightBack + width * width) / 2;

            float subangle = (float)Math.Atan2(width / 2, height / 2);


            float h = (float)(((Math.PI * 2) - heading) + (Math.PI / 2));

            float r1 = h - subangle;
            float r2 = h + ((float)Math.PI / 4) + (float)Math.PI;
            float r3 = h - ((float)Math.PI / 4) + (float)Math.PI;
            float r4 = h + subangle;

            _vertexBuffer[0] = new ColoredVertex(newCenter, pointColor);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r1) * diag, 0, (float)Math.Sin(r1) * diag), pointColor);
            _vertexBuffer[2] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r2) * diagBack, 0, (float)Math.Sin(r2) * diagBack), color);
            _vertexBuffer[3] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r3) * diagBack, 0, (float)Math.Sin(r3) * diagBack), color);
            _vertexBuffer[4] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r4) * diag, 0, (float)Math.Sin(r4) * diag), pointColor);
            _vertexBuffer[5] = _vertexBuffer[1];

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, _vertexBuffer);
        }

        public void DrawSideAttackAgroLine(Clio.Utilities.Vector3 center, float heading, float width, float height, Color color)
        {
            var newCenter = new Vector3(center.X, center.Y, center.Z);

            float diag = (float)Math.Sqrt(height * height + width * width) / 2;
            float subangle = (float)Math.Atan2(width / 2, height / 2);


            float h = (float)(((Math.PI * 2) - heading));

            float r1 = h - subangle;
            float r2 = h + subangle + (float)Math.PI;
            float r3 = h - subangle + (float)Math.PI;
            float r4 = h + subangle;

            _vertexBuffer[0] = new ColoredVertex(newCenter, color);
            _vertexBuffer[1] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r1) * diag, 0, (float)Math.Sin(r1) * diag), color);
            _vertexBuffer[2] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r2) * diag, 0, (float)Math.Sin(r2) * diag), color);
            _vertexBuffer[3] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r3) * diag, 0, (float)Math.Sin(r3) * diag), color);
            _vertexBuffer[4] = new ColoredVertex(newCenter + new Vector3((float)Math.Cos(r4) * diag, 0, (float)Math.Sin(r4) * diag), color);
            _vertexBuffer[5] = _vertexBuffer[1];

            SetDeclaration();
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, 4, _vertexBuffer);
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct ColoredVertex
        {
            public ColoredVertex(Vector3 position, int color)
            {
                Position = position;
                Color = color;
            }

            public ColoredVertex(Vector3 position, Color color)
                : this(position, color.ToArgb())
            {
            }

            public Vector3 Position;
            public int Color;

            public static int Stride => sizeof(ColoredVertex);
            public static VertexFormat Format => VertexFormat.Position | VertexFormat.Diffuse;

            public static VertexDeclaration GetDecl(Device device)
            {
                return new VertexDeclaration(device, new[]
                {
                    new VertexElement(0, 0,
                        DeclarationType.Float3,
                        DeclarationMethod.Default,
                        DeclarationUsage.Position,
                        0),
                    new VertexElement(0, 12,
                        DeclarationType.Color,
                        DeclarationMethod.Default,
                        DeclarationUsage.Color, 0),
                    VertexElement.VertexDeclarationEnd
                });
            }
        }
    }

}