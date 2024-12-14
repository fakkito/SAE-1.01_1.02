using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SAE_1._01_1._02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly double VITESSE_JOUEUR = 3;

        DispatcherTimer timerJeu = new DispatcherTimer();

        Rect joueurHitBox;
        Rect solHitBox;
        Rect obstacleHitBox;
        Rect platformsHitBox;

        bool saut;
        bool toucherSol;

        int force = 20;
        int speed = 50;

        Random rnd = new Random();

        bool gameOver;

        double spriteIndex = 0;

        private static BitmapImage newRunner_01_Droite;
        private static BitmapImage newRunner_01_Gauche;
        private static BitmapImage platforms;

        ImageBrush playerSprite = new ImageBrush();
        ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush obstacleSprite = new ImageBrush();
        ImageBrush plateformeSprite = new ImageBrush();

        int[] obstaclePosition = { 320, 310, 300, 305, 315 }; // Positions à laquelle spawn aléatoirement les obstacles
        int[] imgPlateformePosition = { 260, 250, 240, 245, 255 }; // Positions à laquelle spawn aléatoirement les plateformes


        int score = 0;


        public MainWindow()
        {
            InitializeComponent();
            WindowMenu menu = new WindowMenu();
            menu.ShowDialog();

            InitBitmaps();
            mainCanvas.Focus();

            timerJeu.Tick += GameEngine;
            timerJeu.Interval = TimeSpan.FromMilliseconds(20);

            backgroundSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/fond_ecran_automne.jpg"));

            background.Fill = backgroundSprite;
            background2.Fill = backgroundSprite;
            background3.Fill = backgroundSprite;

            StartGame();
        }

        private void InitBitmaps()
        {
            newRunner_01_Droite = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_01.gif"));
            newRunner_01_Gauche = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_01_gauche.gif"));

            imgNewRunner01_Droite.Source = newRunner_01_Droite;
        }


        private void GameEngine(object? sender, EventArgs e)
        {
            // When launching the game, the player goes down to hit the ground
            Canvas.SetTop(joueur, Canvas.GetTop(joueur) + speed);

            // moves the obstacle horizontally to the left by 5 units.
            Canvas.SetLeft(obstacle, Canvas.GetLeft(obstacle) - 5);

            scoreText.Content = "Score: 0" + score; 

            // Setup 3 Hitboxs
            joueurHitBox = new Rect(Canvas.GetLeft(joueur), Canvas.GetTop(joueur), joueur.Width - 50, joueur.Height);
            obstacleHitBox = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.Width, obstacle.Height);
            solHitBox = new Rect(Canvas.GetLeft(sol), Canvas.GetTop(sol), sol.Width, sol.Height);
            platformsHitBox = new Rect(Canvas.GetLeft(imgPlateforme), Canvas.GetTop(imgPlateforme), imgPlateforme.Width, imgPlateforme.Height);

            // Actions when the player lands on the floor
            if (joueurHitBox.IntersectsWith(solHitBox))
            {
                speed = 0;

                // Position the player just above the ground (sol)
                Canvas.SetTop(joueur, Canvas.GetTop(sol) - joueur.Height + 1);

                saut = false;

                // Vitesse à laquelle les images changent pour faire genre le joueur court
                spriteIndex += 0.5;

                // Si on atteint l'image 8, on repart à la 1
                if (spriteIndex > 8)
                {
                    spriteIndex = 1;
                }

            }

            if (joueurHitBox.IntersectsWith(platformsHitBox))
            {
                speed = 0;

                // Vitesse à laquelle les images changent pour faire genre le joueur court
                spriteIndex += 0.5;

                // Position the player just above the ground (sol)
                Canvas.SetTop(joueur, Canvas.GetTop(imgPlateforme) - joueur.Height + 1);

                saut = false;

                if (spriteIndex > 8)
                {
                    spriteIndex = 1;
                }
            }

            if (saut == true)
            {
                // Reduce the player speed when jumping
                speed = -10;

                // Force limits how far the player can actually jump 
                force -= 1;
            }

            else
            {
                speed = 12;
            }

            if (force < 0)
            {
                saut = false;
            }

            // Si l'obstacle est inférieur à -50 en X alors il est Re Set à 950 en X
            if (Canvas.GetLeft(obstacle) < -50)
            {
                Canvas.SetLeft(obstacle, 950);

                // Find a position for obstacle in Y between the values in the table obstaclePosition[]
                // if 0 = 320, if 1 = 310 etc ...

                Canvas.SetTop(obstacle, obstaclePosition[rnd.Next(0, obstaclePosition.Length)]);

                score += 1;
            }
            if (Canvas.GetLeft(imgPlateforme) < -50)
            {
                Canvas.SetLeft(imgPlateforme, 950);

                // Find a position for obstacle in Y between the values in the table obstaclePosition[]
                // if 0 = 320, if 1 = 310 etc ...

                Canvas.SetTop(imgPlateforme, imgPlateformePosition[rnd.Next(0, imgPlateformePosition.Length)]);

                score += 1;
            }
            if (joueurHitBox.IntersectsWith(obstacleHitBox))
            {
                gameOver = true;

                timerJeu.Stop();
            }

            if (gameOver == true)
            {
                // Visibilité de la HitBox de l'obstacle
                obstacle.Stroke = Brushes.Black;
                obstacle.StrokeThickness = 1;

                // Visibilité de la HitBox de la plateforme
                imgPlateforme.Stroke = Brushes.Blue;
                imgPlateforme.StrokeThickness = 1;

                joueur.Stroke = Brushes.Red;
                joueur.StrokeThickness = 1;

                scoreText.Content = "Score: " + score + " Press Enter to play again !";
            }
            else
            {
                // Visibilité de la HitBox de l'obstacle
                joueur.StrokeThickness = 0;
                obstacle.StrokeThickness = 0;

                // Visibilité de la HitBox de la plateforme
                imgPlateforme.StrokeThickness = 0;

            }
            RunSprite(spriteIndex);
            // moves the obstacle horizontally to the left by 5 units.
            Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) - 5);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && gameOver == true)
            {
                StartGame();
            }

            // Pour faire défiler les fonds quand on va à droite
            if (e.Key == Key.Right && gameOver == false)
            {
                imgNewRunner01_Droite.Source = newRunner_01_Droite;
                Canvas.SetLeft(imgNewRunner01_Droite, Canvas.GetLeft(imgNewRunner01_Droite) + 10);
                if (Canvas.GetLeft(imgNewRunner01_Droite) > this.ActualWidth - imgNewRunner01_Droite.Width)
                    Canvas.SetLeft(imgNewRunner01_Droite, this.ActualWidth - imgNewRunner01_Droite.Width);


                // FOR THE BACKGROUND TO MOVE 

                Canvas.SetLeft(background, Canvas.GetLeft(background) - VITESSE_JOUEUR); // Vitesse
                Canvas.SetLeft(background2, Canvas.GetLeft(background2) - VITESSE_JOUEUR); // Vitesse 
                Canvas.SetLeft(background3, Canvas.GetLeft(background3) - VITESSE_JOUEUR); // Vitesse 


                // Loop the backgrounds when it goes off-screen

                if (Canvas.GetLeft(background) < -background.ActualWidth)
                {
                    Canvas.SetLeft(background, 0);
                    Canvas.SetLeft(background2, background.ActualWidth);
                    Canvas.SetLeft(background3, -background.ActualWidth);
                }
                Console.WriteLine(Canvas.GetLeft(background));

            }

            // Pour faire défiler les fonds quand on va à gauche
            if (e.Key == Key.Left && gameOver == false)
            {
                joueur.Fill = new ImageBrush(newRunner_01_Gauche); // When i press Left, the image is "newRunner_01_Gauche.gif"
                imgNewRunner01_Droite.Source = newRunner_01_Gauche;
                Canvas.SetLeft(imgNewRunner01_Droite, Canvas.GetLeft(imgNewRunner01_Droite) - 10);
                if (Canvas.GetLeft(imgNewRunner01_Droite) < 0)
                    Canvas.SetLeft(imgNewRunner01_Droite, 0);

                // FOR THE BACKGROUND TO MOVE 

                Canvas.SetLeft(background, Canvas.GetLeft(background) + VITESSE_JOUEUR); // Vitesse
                Canvas.SetLeft(background3, Canvas.GetLeft(background3) + VITESSE_JOUEUR); // Vitesse
                Canvas.SetLeft(background2, Canvas.GetLeft(background2) + VITESSE_JOUEUR); // Vitesse


                // Loop the backgrounds when it goes off-screen

                if (Canvas.GetLeft(background3) > 0)
                {
                    Canvas.SetLeft(background, 0);
                    Canvas.SetLeft(background2, background.ActualWidth);
                    Canvas.SetLeft(background3, -background.ActualWidth);
                }

            }
            // When Escape is press, the timer (timerJeu) stops.
            if (e.Key == Key.Escape)
            {
                if (timerJeu.IsEnabled)
                    timerJeu.Stop();
                else
                    timerJeu.Start();

            }
                #if DEBUG
                Console.WriteLine(e.Key);    // Affichage dans la console de la "Key pressed"
                #endif
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

            // if the space key is pressed AND jumping boolean is true AND player y location is above 260 pixels
            if (e.Key == Key.Space && !saut && !joueurHitBox.IntersectsWith(obstacleHitBox))
            {
              saut = true;     
              force = 15;
              speed = -12;

              // change the player sprite so it looks like he's jumping
              playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_02.gif")); // newRunner_02 use when jumping 

                
            }
            
        }

        private void StartGame()
        {
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);
            Canvas.SetLeft(background3, -1262);

            // Set the player on the ground when starting
            Canvas.SetLeft(joueur, 46);
            Canvas.SetTop(joueur, 270);

            Canvas.SetLeft(obstacle, 950);
            Canvas.SetTop(obstacle, 310);

            Canvas.SetLeft(imgPlateforme, 300);
            Canvas.SetTop(imgPlateforme, 250);


            RunSprite(1);

            obstacleSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/block.png"));
            obstacle.Fill = obstacleSprite;

            plateformeSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/platforms.png"));
            imgPlateforme.Fill = plateformeSprite;

            saut = false;
            gameOver = false;
            score = 0;

            scoreText.Content = "Score: " + score;

            timerJeu.Start();

        }

        private void RunSprite(double i)
        {

            // DIFFERENTS CASE TO MAKE THE PLAYER SPRINT SMOOTHLY            
            switch (i)
            {
                case 1:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_01.gif"));
                    break;
                case 2:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_02.gif"));
                    break;
                case 3:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_03.gif"));
                    break;
                case 4:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_04.gif"));
                    break;
                case 5:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_05.gif"));
                    break;
                case 6:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_06.gif"));
                    break;
                case 7:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_07.gif"));
                    break;
                case 8:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_08.gif"));
                    break;
            }

            joueur.Fill = playerSprite;

        }

        /*
        private void MoveGameElements(string direction)
        {
            foreach(Control x in this.Controls)
            {
                if (x is PictureBox)
            }


        }
        */
    }
}
    
