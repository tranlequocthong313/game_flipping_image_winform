using BTH_TH_1.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BTH_TH_1
{
    public partial class Form1 : Form
    {
        private int size;
        private Graphics graphics;
        private Bitmap canvas;
        private int[] cells;
        private string[] images;
        private Random random;
        private Rectangle rectangle;
        private SolidBrush brush;
        private Pen pen;
        private GameState gameState;
        private int[] flippingStates;
        private int flippingCell1, flippingCell2;
        private Point flippingPoint1, flippingPoint2;
        private int flippedCounter;
        private bool isWattingForAllUnflip;

        private enum GameState
        {
            FlipOne, FlipNothing, GameOver
        }

        public Form1()
        {
            InitializeComponent();
            canvas = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            graphics = Graphics.FromImage(canvas);
            rectangle = new Rectangle();
            rectangle.Width = ClientSize.Width / 4;
            rectangle.Height = ClientSize.Height / 4;
            brush = new SolidBrush(Color.Black);
            pen = new Pen(Color.DarkGray, 4);
            pen.Alignment = PenAlignment.Inset;
            random = new Random();
            size = 16;
            cells = new int[size];
            images = Directory.GetFiles(Application.StartupPath + "/Images");
            timer1.Interval = 500;
            isWattingForAllUnflip = false;

            SetupBoard();
        }

        private void SetupBoard()
        {
            ResetAllStates();

            InsertImages();

            ShuffleImages();

            DrawCells();
        }

        private void ResetAllStates()
        {
            rectangle.X = 0;
            rectangle.Y = 0;
            gameState = GameState.FlipNothing;
            flippedCounter = 0;
            flippingStates = new int[size];
            flippingCell1 = -1;
            flippingCell2 = -1;
            isWattingForAllUnflip = false;
        }

        private void InsertImages()
        {
            for (int i = 0; i <= size - 2; i += 2)
            {
                var imageIndex = random.Next(0, images.Length);
                cells[i] = imageIndex;
                cells[i + 1] = imageIndex;
            }
        }

        private void ShuffleImages()
        {
            for (int i = cells.Length - 1; i > 0; i--)
            {
                var randomPosition = random.Next(0, i + 1);
                (cells[i], cells[randomPosition]) = (cells[randomPosition], cells[i]);
            }
        }

        private void DrawCells()
        {
            for (int i = 0; i < 4; i++)
            {
                rectangle.X = 0;
                for (int j = 0; j < 4; j++)
                {
                    rectangle.X = j * (ClientSize.Width / 4);
                    rectangle.Y = i * (ClientSize.Height / 4);

                    DrawFilledRectangleWithBorder();
                }
            }
        }

        private void DrawFilledRectangleWithBorder()
        {
            graphics.FillRectangle(brush, rectangle);
            graphics.DrawRectangle(pen, rectangle);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(canvas, 0, 0);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isWattingForAllUnflip) return;

            var imageIndex = FlipImage(e.Location, true);
            if (imageIndex == -1) return;

            if (gameState == GameState.FlipNothing)
            {
                flippingCell1 = imageIndex;
                flippingPoint1 = e.Location;
                gameState = GameState.FlipOne;
            }
            else if (gameState == GameState.FlipOne)
            {
                flippingCell2 = imageIndex;
                flippingPoint2 = e.Location;

                if (flippingCell1 == flippingCell2)
                {
                    flippedCounter += 2;
                    if (flippedCounter == size)
                    {
                        gameState = GameState.GameOver;
                        timer1.Start();
                        return;
                    }
                }
                else
                {
                    isWattingForAllUnflip = true;
                    timer1.Start();
                }

                gameState = GameState.FlipNothing;
            }
            Invalidate();
        }

        private int FlipImage(Point mouseLocation, bool isFlippingUp)
        {
            var rowIndex = (ClientSize.Height - mouseLocation.Y) / (ClientSize.Height / 4);
            var colIndex = (ClientSize.Width - mouseLocation.X) / (ClientSize.Width / 4);

            var cellRowIndex = 4 - rowIndex - 1;
            var cellColIndex = 4 - colIndex - 1;

            var arrIndex = cellRowIndex * 4 + cellColIndex;

            if (isFlippingUp && flippingStates[arrIndex] != 0) return -1;
            if (!isFlippingUp && flippingStates[arrIndex] == 0) return -1;

            rectangle.X = cellColIndex * rectangle.Width;
            rectangle.Y = cellRowIndex * rectangle.Height;

            if (isFlippingUp)
            {
                // TODO: This needs optimizing
                var image = new Bitmap(Image.FromFile(images[cells[arrIndex]]), rectangle.Width, rectangle.Height);
                graphics.DrawImage(image, rectangle);
                flippingStates[arrIndex] = 1;
            }
            else
            {
                flippingStates[arrIndex] = 0;
                DrawFilledRectangleWithBorder();
            }

            return cells[arrIndex];
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (gameState == GameState.GameOver) SetupBoard();
            else FlipTwoUppingImagesDown();

            isWattingForAllUnflip = false;
            timer1.Stop();
            Invalidate();
        }

        private void FlipTwoUppingImagesDown()
        {
            FlipImage(flippingPoint1, false);
            FlipImage(flippingPoint2, false);
        }
    }
}