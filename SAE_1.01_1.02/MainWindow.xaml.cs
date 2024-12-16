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

        // HitBoxs
        Rect joueurHitBox;
        Rect solHitBox;
        Rect ennemiHitBox;
        Rect platformsHitBox;
        Rect fromageHitBox;
        Rect charcuterieHitBox;

        bool saut;

        int force = 20; // Equal the gravity
        int speed = 50; // Force qui est opposé à la gravité

        Random rnd = new Random();

        bool gameOver;
        bool isPaused = false;

        double spriteIndex = 0;

        private static BitmapImage newRunner_01_Droite;
        private static BitmapImage newRunner_01_Gauche;

        // private static BitmapImage platforms;

        ImageBrush playerSprite = new ImageBrush();
        ImageBrush playerSpriteGauche = new ImageBrush();
        ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush ennemiSprite = new ImageBrush();
        ImageBrush plateformeSprite = new ImageBrush();

        int[] ennemiPosition = { 315 }; // Positions à laquelle spawn les ennemis en hauteur
        int[] imgPlateformePosition = { 260, 250, 240, 245, 255 }; // Positions à laquelle spawn aléatoirement les plateformes

        int tempsEcoule = 0;
        int temps = 500;
        int compteurFromages = 0;
        int compteurCharcuteries = 0;

        public MainWindow()
        {
            InitializeComponent();
            WindowMenu menu = new WindowMenu();
            menu.ShowDialog();

            InitBitmaps();
            mainCanvas.Focus(); // Permet de capturer immédiatement les événements clavier

            timerJeu.Tick += GameEngine;
            timerJeu.Interval = TimeSpan.FromMilliseconds(24);

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
            if (isPaused)
            {
                tempsText.Content = "GAME PAUSED\nPress Escape to Resume";
                return;
            }
            tempsEcoule++;
            // Décrémenter le temps toutes les 50 appels (environ toutes les secondes)
            if (tempsEcoule % 50 == 0)
            {
                temps--;
                tempsText.Content = "Temps : " + temps;

                if (temps <= 0)
                {
                    gameOver = true;
                    timerJeu.Stop();
                }
            }

            // When launching the game, the player goes down to hit the ground
            Canvas.SetTop(joueur, Canvas.GetTop(joueur) + speed);

            // moves the ennemi horizontally to the left by 5 units.
            Canvas.SetLeft(ennemi, Canvas.GetLeft(ennemi) - 5);

            tempsText.Content = "Temps : " + temps; 

            // Setup 3 Hitboxs
            joueurHitBox = new Rect(Canvas.GetLeft(joueur), Canvas.GetTop(joueur), joueur.Width - 15, joueur.Height);
            ennemiHitBox = new Rect(Canvas.GetLeft(ennemi), Canvas.GetTop(ennemi),  ennemi.Width, ennemi.Height);
            solHitBox = new Rect(Canvas.GetLeft(sol), Canvas.GetTop(sol), sol.Width, sol.Height);
            platformsHitBox = new Rect(Canvas.GetLeft(imgPlateforme), Canvas.GetTop(imgPlateforme), imgPlateforme.Width, imgPlateforme.Height);
            fromageHitBox = new Rect(Canvas.GetLeft(fromage), Canvas.GetTop(fromage), fromage.Width, fromage.Height);
            charcuterieHitBox = new Rect(Canvas.GetLeft(charcuterie), Canvas.GetTop(charcuterie), charcuterie.Width, charcuterie.Height);

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

            // Si l'ennemi est inférieur à -50 en X alors il est Re Set à 950 en X
            if (Canvas.GetLeft(ennemi) < -50)
            {
                Canvas.SetLeft(ennemi, 950);

                // Find a position for ennemi in Y between the values in the table oennemiPosition[]
                // if 0 = 320, if 1 = 310 etc ...

                Canvas.SetTop(ennemi, ennemiPosition[rnd.Next(0, ennemiPosition.Length)]);

            }
            if (Canvas.GetLeft(imgPlateforme) < -50)
            {
                // Set the plateforme at 950 in X
                Canvas.SetLeft(imgPlateforme, 950);

                // Find a position for platefrome in Y between the values in the table ennemiPosition[]
                // if 0 = 320, if 1 = 310 etc ...

                Canvas.SetTop(imgPlateforme, imgPlateformePosition[rnd.Next(0, imgPlateformePosition.Length)]);

            }
            if (joueurHitBox.IntersectsWith(ennemiHitBox))
            {
                gameOver = true;

                timerJeu.Stop();
            }

            if (joueurHitBox.IntersectsWith(fromageHitBox))
            {
                // On remet le fromage à sa position initiale
                Canvas.SetLeft(fromage, 950);
                Canvas.SetTop(fromage, 200);
                compteurFromages += 1;
                compteurFromage.Content = compteurFromages;
            }

            if (joueurHitBox.IntersectsWith(charcuterieHitBox))
            {
                // On remet la charcuterie à sa position initiale
                Canvas.SetLeft(charcuterie, 1150);
                Canvas.SetTop(charcuterie, 200);
                compteurCharcuteries += 1;
                compteurCharcuterie.Content = compteurCharcuteries;
            }

            if (gameOver == true)
            {
                // Visibilité de la HitBox de l'ennemi
                ennemi.Stroke = Brushes.Black;
                ennemi.StrokeThickness = 1;

                // Visibilité de la HitBox de la plateforme
                imgPlateforme.Stroke = Brushes.Blue;
                imgPlateforme.StrokeThickness = 1;

                // Visibilité de la HitBox du joueur
                joueur.Stroke = Brushes.Red;
                joueur.StrokeThickness = 1;

                // On remet les compteurs à 0
                compteurFromages = 0;
                compteurCharcuteries = 0;
                tempsText.Content = "Temps : " + temps + " \nPress Enter to play again !";
                compteurFromage.Content = compteurFromages;
                compteurCharcuterie.Content = compteurCharcuteries;
            }
            else
            {
                // Visibilité de la HitBox du joueur
                joueur.StrokeThickness = 0;

                // Visibilité de la HitBox de l'ennemi
                ennemi.StrokeThickness = 0;

                // Visibilité de la HitBox de la plateforme
                imgPlateforme.StrokeThickness = 0;

            }
            RunSpriteEnnemi(spriteIndex);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter && gameOver == true)
            {
                StartGame();
            }

            // Si le jeu est en pause, on peut le reprendre en appuyant sur la touche Echap
            if (isPaused)
            {
                if (e.Key == Key.Escape)
                {
                    // On remet le jeu en route
                    isPaused = false;
                    timerJeu.Start();
                }
                return;
            }
            // When Escape is press, the timer (timerJeu) stops.
            if (e.Key == Key.Escape)
            {
                    isPaused = true;
                    timerJeu.Stop();
                return;
            }
                #if DEBUG
                Console.WriteLine(e.Key);    // Affichage dans la console de la "Key pressed"
                #endif

            if (!gameOver && !isPaused)
            {
                // Pour faire défiler les éléments quand on va à droite
                if (e.Key == Key.Right && gameOver == false)
                {
                    RunSprite(spriteIndex);
                    imgNewRunner01_Droite.Source = newRunner_01_Droite;
                    Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) + VITESSE_JOUEUR);

                    // Move the actual plateforme at the same speed as the player but inversely
                    Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) - VITESSE_JOUEUR);

                    // Move the actual fromage at the same speed as the player but inversely
                    Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) - VITESSE_JOUEUR);

                    // Move the actual charcuterie at the same speed as the player but inversely
                    Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) - VITESSE_JOUEUR);

                    // Prevent player from moving off the left side of the screen
                    if (Canvas.GetLeft(joueur) > this.ActualWidth - joueur.Width)
                        Canvas.SetLeft(joueur, this.ActualWidth - joueur.Width);

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

                // Pour faire défiler les éléments quand on va à gauche
                if (e.Key == Key.Left && gameOver == false)
                {
                    RunSpriteGauche(spriteIndex);
                    joueur.Fill = new ImageBrush(newRunner_01_Gauche); // When i press Left, the image is "newRunner_01_Gauche.gif"
                    imgNewRunner01_Droite.Source = newRunner_01_Gauche;

                    // Move the actual player character
                    Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) - VITESSE_JOUEUR);

                    // Move the actual plateforme at the same speed as the player but inversely
                    Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) + VITESSE_JOUEUR);

                    // Move the actual fromage at the same speed as the player but inversely
                    Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) + VITESSE_JOUEUR);

                    // Move the actual charcuterie at the same speed as the player but inversely
                    Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) + VITESSE_JOUEUR);


                    // Prevent player from moving off the left side of the screen
                    if (Canvas.GetLeft(joueur) < 0)
                        Canvas.SetLeft(joueur, 0);

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
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // if the space key is pressed AND jumping boolean is false AND playerHitBox touch the ennemieHitBox
            if (e.Key == Key.Space && !saut && !joueurHitBox.IntersectsWith(ennemiHitBox))
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
            isPaused = false;

            // Positions where are set the backgrounds
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);
            Canvas.SetLeft(background3, -1262);

            // Set the player on the ground when starting
            Canvas.SetLeft(joueur, 46); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(joueur) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(joueur, 270);

            // Set the ennemi
            Canvas.SetLeft(ennemi, 950); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(ennemi) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(ennemi, 310);

            // Set the plateforme 
            Canvas.SetLeft(imgPlateforme, 300); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(imgPlateforme) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(imgPlateforme, 250);

            // Set the fromage 
            Canvas.SetLeft(fromage, 200); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(fromage) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(fromage,  200);

            // Set the charcuterie 
            Canvas.SetLeft(charcuterie, 300); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(charcuterie) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(charcuterie, 200);
            RunSprite(1);

            ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_1.png"));
            ennemi.Fill = ennemiSprite;

            plateformeSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/platforms.png"));
            imgPlateforme.Fill = plateformeSprite;

            saut = false;
            gameOver = false;
            tempsEcoule = 0;
            temps = 500;
            compteurFromage.Content = 0;
            compteurCharcuterie.Content = 0;

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
        private void RunSpriteGauche(double i)
        {

            // DIFFERENTS CASE TO MAKE THE PLAYER SPRINT SMOOTHLY            
            switch (i)
            {
                case 1:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_01_Gauche.gif"));
                    break;
                case 2:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_02_Gauche.gif"));
                    break;
                case 3:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_03_Gauche.gif"));
                    break;
                case 4:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_04_Gauche.gif"));
                    break;
                case 5:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_05_Gauche.gif"));
                    break;
                case 6:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_06_Gauche.gif"));
                    break;
                case 7:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_07_Gauche.gif"));
                    break;
                case 8:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/newRunner_08_Gauche.gif"));
                    break;
            }

            joueur.Fill = playerSpriteGauche;

        }
        private void RunSpriteEnnemi(double i)
        {

            // DIFFERENTS CASE TO MAKE THE PLAYER SPRINT SMOOTHLY            
            switch (i)
            {
                case 1:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_1.png"));
                    break;
                case 2:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_2.png"));
                    break;
                case 3:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_3.png"));
                    break;
                case 4:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_4.png"));
                    break;
                case 5:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_5.png"));
                    break;
                case 6:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_6.png"));
                    break;
                case 7:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_7.png"));
                    break;
                case 8:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_8.png"));
                    break;
            }

            ennemi.Fill = ennemiSprite;

        }
    }
}
    
