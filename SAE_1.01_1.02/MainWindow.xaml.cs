using System;
using System.Diagnostics.Eventing.Reader;
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
        // Vitesse
        static readonly double VITESSE_JOUEUR = 5;
        static readonly double VITESSE_ENNEMI = 2;

        // Constantes pour les difficultés
        static readonly int OBJETS_FACILE = 5;
        static readonly int OBJETS_MOYEN = 10;
        static readonly int OBJETS_DIFFICILE = 15;

        // Variables pour stocker les objectifs actuels
        private int objectifFromages;
        private int objectifCharcuteries;
        private int objectifPatates;
        private int objectifSalades;

        //Deplacements 
        private bool goDroite, goGauche;

        DispatcherTimer timerJeu = new DispatcherTimer();

        // HitBoxs
        Rect joueurHitBox;
        Rect solHitBox;
        Rect ennemiHitBox;
        Rect platformsHitBox;
        Rect fromageHitBox;
        Rect charcuterieHitBox;
        Rect patateHitBox;
        Rect saladeHitBox;
        Rect bossHitBox;
        Rect armeHitBox;

        bool saut;

        int force = 20; // Equal the gravity
        int speed = 50; // Force qui est opposé à la gravité
        int widthSpawnElement = 800;  // Largeur de la fenêtre
        int heightSpawnElement = 450; // Hauteur de la fenêtre
        int compteurBackground = 0; // compteur du nombre de fois que se reset les backgrounds

        Random rnd = new Random();

        bool gameOver;
        bool isPaused = false;

        double spriteIndex = 0;
        double moveDirectionX = 0;

        private static BitmapImage Run_1_Droit;
        private static BitmapImage Run_1_Gauch;

        // private static BitmapImage platforms;

        ImageBrush playerSprite = new ImageBrush();
        ImageBrush playerSpriteGauche = new ImageBrush();
        //ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush ennemiSprite = new ImageBrush();

        // Positions à laquelle spawn aléatoirement les éléments dans une tranche donnée. Qui ensuite utilisée pour SetTop soit en Y
        int[] imgPlateformePositionY = { 140, 150, 160, 170 };
        int[] positionElementY = { 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270 };

        // Positions à laquelle spawn aléatoirement les éléments dans une tranche donnée. Qui ensuite utilisée pour SetTop soit en X
        int[] positionElementX = { 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320, 330, 340, 350, 360, 370, 380, 390, 400, 410, 420, 430, 440, 450, 460, 470, 480, 490, 500, 510, 520, 530, 540, 550, 560, 570, 580, 590, 600, 610, 620, 630, 640, 650 };

        int tempsEcoule;
        int temps;
        int compteurFromages;
        int compteurCharcuteries;
        int compteurPatates;
        int compteurSalades;

        private double distanceParcourue = 0;
        private double dernierePositionX;

        private static MediaPlayer musiqueJeu;

        public MainWindow()
        {
            InitializeComponent();
            WindowMenu menu = new WindowMenu();
            menu.ShowDialog();

            InitBitmaps();
            mainCanvas.Focus(); // Permet de capturer immédiatement les événements clavier

            timerJeu.Tick += GameEngine;
            timerJeu.Interval = TimeSpan.FromMilliseconds(24);

            //backgroundSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/fond_ecran_automne.jpg"));

            //background.Fill = backgroundSprite;
            //background2.Fill = backgroundSprite;
            //background3.Fill = backgroundSprite;

            // Récupération de la position de départ du joueur
            dernierePositionX = Canvas.GetLeft(joueur);

            //InitMusique();

            StartGame();

        }


        private void InitBitmaps()
        {
            Run_1_Droit = new BitmapImage(new Uri("pack://application:,,,/img/Run_1.png"));
            Run_1_Gauch = new BitmapImage(new Uri("pack://application:,,,/img/Run_1_Gauche.png"));

            Run_1_Droite.Source = Run_1_Droit;
        }

        private void GameEngine(object? sender, EventArgs e)
        {
            IA_ENNEMI();

#if DEBUG
            //Console.WriteLine(compteurBackground);
#endif
            // Dès que le jeu se lance on écoule le temps
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
            MOUVEMENT_JOUEUR();

            // When launching the game, the player goes down to hit the ground
            Canvas.SetTop(joueur, Canvas.GetTop(joueur) + speed);

            // moves the ennemi horizontally to the left by 5 units.
            //Canvas.SetLeft(ennemi, Canvas.GetLeft(ennemi)); // Si pas tester de mettre dans  goDroite et goGauche

            tempsText.Content = "Temps : " + temps;

            // Setup 3 Hitboxs
            joueurHitBox = new Rect(Canvas.GetLeft(joueur), Canvas.GetTop(joueur), joueur.Width - 15, joueur.Height);
            ennemiHitBox = new Rect(Canvas.GetLeft(ennemi), Canvas.GetTop(ennemi), ennemi.Width, ennemi.Height);
            solHitBox = new Rect(Canvas.GetLeft(sol), Canvas.GetTop(sol), sol.Width, sol.Height);
            platformsHitBox = new Rect(Canvas.GetLeft(imgPlateforme), Canvas.GetTop(imgPlateforme), imgPlateforme.Width, imgPlateforme.Height);
            fromageHitBox = new Rect(Canvas.GetLeft(fromage), Canvas.GetTop(fromage), fromage.Width, fromage.Height);
            charcuterieHitBox = new Rect(Canvas.GetLeft(charcuterie), Canvas.GetTop(charcuterie), charcuterie.Width, charcuterie.Height);
            patateHitBox = new Rect(Canvas.GetLeft(patate), Canvas.GetTop(patate), patate.Width, patate.Height);
            bossHitBox = new Rect(0, 0, 0, 0);
            armeHitBox = new Rect(Canvas.GetLeft(Arme), Canvas.GetTop(Arme), Arme.Width, Arme.Height);
            saladeHitBox = new Rect(Canvas.GetLeft(salade), Canvas.GetTop(salade), salade.Width, salade.Height);

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

                // Find a position for ennemi in Y 

                Canvas.SetTop(ennemi, 315);

            }
            if (Canvas.GetLeft(imgPlateforme) < -50)
            {
                // Set the plateforme at 950 in X
                Canvas.SetLeft(imgPlateforme, 950);

                // Find a position for platefrome in Y between the values in the table ennemiPosition[]
                // if 0 = 320, if 1 = 310 etc ...

                Canvas.SetTop(imgPlateforme, imgPlateformePositionY[rnd.Next(0, imgPlateformePositionY.Length)]);

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
                MettreAJourCompteurs();
            }

            if (joueurHitBox.IntersectsWith(charcuterieHitBox))
            {
                // On remet la charcuterie à sa position initiale
                Canvas.SetLeft(charcuterie, 1150);
                Canvas.SetTop(charcuterie, 200);
                compteurCharcuteries += 1;
                MettreAJourCompteurs();
            }

            if (joueurHitBox.IntersectsWith(patateHitBox))
            {
                // On remet la patate à sa position initiale
                Canvas.SetLeft(patate, 1350);
                Canvas.SetTop(patate, 200);
                compteurPatates += 1;
                MettreAJourCompteurs();
            }

            if (joueurHitBox.IntersectsWith(saladeHitBox))
            {
                // On remet la salade à sa position initiale
                Canvas.SetLeft(salade, 1550);
                Canvas.SetTop(salade, 200);
                compteurSalades += 1;
                MettreAJourCompteurs();
            }

            // Vérification des objectifs après mise à jour des compteurs
            if (compteurFromages >= objectifFromages &&
                compteurCharcuteries >= objectifCharcuteries &&
                compteurPatates >= objectifPatates &&
                compteurSalades >= objectifSalades)
            {
                gameOver = true;
                timerJeu.Stop();
                MessageBox.Show("Félicitations! Vous avez atteint tous les objectifs!");
            }
            if (joueurHitBox.IntersectsWith(armeHitBox))
            {
                Canvas.SetLeft(Arme, Canvas.GetLeft(joueur) + 40);
                Canvas.SetTop(Arme, Canvas.GetTop(joueur) + 35);

            }

            if (joueurHitBox.IntersectsWith(bossHitBox))
            {
                gameOver = true;
                timerJeu.Stop();
                Console.WriteLine("Boss touché !");
            }
            ////if (compteurCharcuteries < 1 && compteurFromages < 1 && compteurPatates < 1 && compteurSalades < 1)
            ////{
            ////    bossHitBox = new Rect(0, 0, 0, 0);
            ////    boss.Opacity = 0;

            ////}
            ////else
            ////{
            ////    bossHitBox = new Rect(Canvas.GetLeft(boss), Canvas.GetTop(boss), boss.ActualWidth, boss.ActualHeight);
            ////    boss.Opacity = 100;
            ////    Canvas.SetLeft(boss, 650);
            ////    Canvas.SetTop(boss, 221);
            ////    Console.WriteLine($"Boss spawning. Background counter : {compteurBackground}");
            ////}
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

                // Visibilité de la HitBox du Boss
                boss.Opacity = 100;
                boss.Stroke = Brushes.Red;
                boss.StrokeThickness = 1;

                // On remet les compteurs à 0
                compteurFromages = 0;
                compteurCharcuteries = 0;
                compteurPatates = 0;
                compteurSalades = 0;

                tempsText.Content = "Temps : " + temps + " \nAppuyer entrer pour rejouer !";

                compteurFromage.Content = compteurFromages;
                compteurCharcuterie.Content = compteurCharcuteries;
                compteurPatate.Content = compteurPatates;
                compteurSalade.Content = compteurSalades;
            }
            else
            {
                // Visibilité de la HitBox du joueur
                joueur.StrokeThickness = 0;

                // Visibilité de la HitBox de l'ennemi
                ennemi.StrokeThickness = 0;

                // Visibilité de la HitBox de la plateforme
                imgPlateforme.StrokeThickness = 0;

                // Visibilité de la HitBox du Boss
                boss.StrokeThickness = 0;
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
                if (e.Key == Key.Right)
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
            if (e.Key == Key.Right)
            {
                goDroite = true;
            }
            if (e.Key == Key.Left)
            {
                ////if (Canvas.GetLeft(joueur) > 0) // Vérification de la limite gauche
                ////{
                goGauche = true;
                ////}
            }


        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                goDroite = false;
            }
            if (e.Key == Key.Left)
            {
                goGauche = false;
            }

            // if the space key is pressed AND jumping boolean is false AND playerHitBox touch the ennemieHitBox
            if (e.Key == Key.Space && !saut && !joueurHitBox.IntersectsWith(ennemiHitBox))
            {
                saut = true;
                force = 15;
                speed = -12;

                // change the player sprite so it looks like he's jumping
                ////playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img prof/newRunner_02.gif")); // newRunner_02 use when jumping 

            }

        }

        // Où se place les éléments sur la fenêtre dès le début
        private void StartGame()
        {

            isPaused = false;

            // Positions where are set the backgrounds
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);
            Canvas.SetLeft(background3, -1262);

            // Set the player on the ground when starting
            Canvas.SetLeft(joueur, 46); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(joueur) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(joueur, 201);

            // Set the ennemi
            Canvas.SetLeft(ennemi, 450);
            Canvas.SetTop(ennemi, 254);

            // Set the plateforme 
            Canvas.SetLeft(imgPlateforme, positionElementX[rnd.Next(0, positionElementX.Length)]);
            Canvas.SetTop(imgPlateforme, imgPlateformePositionY[rnd.Next(0, imgPlateformePositionY.Length)]);

            // Set the fromage 
            Canvas.SetLeft(fromage, positionElementX[rnd.Next(0, positionElementX.Length)]);
            Canvas.SetTop(fromage, positionElementY[rnd.Next(0, positionElementY.Length)]);

            // Set the charcuterie 
            Canvas.SetLeft(charcuterie, positionElementX[rnd.Next(0, positionElementX.Length)]);
            Canvas.SetTop(charcuterie, positionElementY[rnd.Next(0, positionElementY.Length)]);

            // Set the patate 
            Canvas.SetLeft(patate, positionElementX[rnd.Next(0, positionElementX.Length)]);
            Canvas.SetTop(patate, positionElementY[rnd.Next(0, positionElementY.Length)]);

            // Set the salade 
            Canvas.SetLeft(salade, positionElementX[rnd.Next(0, positionElementX.Length)]);
            Canvas.SetTop(salade, positionElementY[rnd.Next(0, positionElementY.Length)]);

            // Set the Boss
            Canvas.SetLeft(boss, 700);
            Canvas.SetTop(boss, 131);
            boss.Opacity = 0;
            boss.Width = 108;
            boss.Height = 93;

            // Set the Arme
            Canvas.SetLeft(Arme, 200);
            Canvas.SetTop(Arme, 231);

            ////RunSprite(1);

            ////ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_1.png"));
            ////ennemi.Fill = ennemiSprite;

            saut = false;
            gameOver = false;
            tempsEcoule = 0;
            temps = 180;
            //compteurFromage.Content = 0;
            //compteurCharcuterie.Content = 0;
            //compteurPatate.Content = 0;
            //compteurSalade.Content = 0;

            // Réinitialiser les compteurs
            compteurFromages = 0;
            compteurCharcuteries = 0;
            compteurPatates = 0;
            compteurSalades = 0;

            // Mettre à jour l'affichage
            MettreAJourCompteurs();

            timerJeu.Start();

        }

        private void RunSprite(double i)
        {

            // DIFFERENTS CASE TO MAKE THE PLAYER SPRINT SMOOTHLY            
            switch (i)
            {
                case 1:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_1.png"));
                    break;
                case 2:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_2.png"));
                    break;
                case 3:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_3.png"));
                    break;
                case 4:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_4.png"));
                    break;
                case 5:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_5.png"));
                    break;
                case 6:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_6.png"));
                    break;
                case 7:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_7.png"));
                    break;
                case 8:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_8.png"));
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
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_1_Gauche.png"));
                    break;
                case 2:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_2_Gauche.png"));
                    break;
                case 3:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_3_Gauche.png"));
                    break;
                case 4:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_4_Gauche.png"));
                    break;
                case 5:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_5_Gauche.png"));
                    break;
                case 6:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_6_Gauche.png"));
                    break;
                case 7:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_7_Gauche.png"));
                    break;
                case 8:
                    playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/Run_8_Gauche.png"));
                    break;
            }

            joueur.Fill = playerSprite;

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

        private void RunSpriteEnnemiDroite(double i)
        {

            // DIFFERENTS CASE TO MAKE THE PLAYER SPRINT SMOOTHLY            
            switch (i)
            {
                case 1:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_1_Droite.png"));
                    break;
                case 2:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_2_Droite.png"));
                    break;
                case 3:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_3_Droite.png"));
                    break;
                case 4:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_4_Droite.png"));
                    break;
                case 5:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_5_Droite.png"));
                    break;
                case 6:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_6_Droite.png"));
                    break;
                case 7:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_7_Droite.png"));
                    break;
                case 8:
                    ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/image_8_Droite.png"));
                    break;
            }

            ennemi.Fill = ennemiSprite;

        }

        protected override void OnKeyDown(KeyEventArgs e) // pour acceder a la page en appuyant sur une touche
        {

            if (e.Key == Key.Escape)
            {
                // Ouvrir le menu
                WindowMenuEchap windowMenuEchap = new WindowMenuEchap();
                windowMenuEchap.ShowDialog();
            }
        }

        private void IA_ENNEMI()
        {
#if DEBUG
            //Console.WriteLine(Canvas.GetLeft(ennemi));
            //Console.WriteLine(Canvas.GetTop(ennemi));
            //Console.WriteLine(Canvas.GetLeft(joueur));
            //Console.WriteLine(Canvas.GetTop(joueur));
#endif


            // Calculer la direction de l'ennemi vers le joueur sur les axes X et Y
            double directionX = Canvas.GetLeft(joueur) - Canvas.GetLeft(ennemi);  // Direction horizontale


            // Calculer la vitesse de déplacement de l'ennemi
            moveDirectionX = Math.Sign(directionX) * VITESSE_ENNEMI - 1;  // Se déplace horizontalement


            // Déplacer l'ennemi horizontalement
            Canvas.SetLeft(ennemi, Canvas.GetLeft(ennemi) + moveDirectionX);

            // Pour debug
            Console.WriteLine("Le suit");

            if (Canvas.GetLeft(joueur) > Canvas.GetLeft(ennemi))
            {
                RunSpriteEnnemiDroite(spriteIndex);
            }
            else
            {
                RunSpriteEnnemi(spriteIndex);
            }

        }
        private void MOUVEMENT_JOUEUR()
        {
            if (goDroite)
            {
                // Mouvement vers la droite
                Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) + VITESSE_JOUEUR);
                RunSprite(spriteIndex);
                Run_1_Droite.Source = Run_1_Droit;

                // Déplacements inverses des autres éléments
                Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) - VITESSE_JOUEUR);
                Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) - VITESSE_JOUEUR);
                Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) - VITESSE_JOUEUR);
                Canvas.SetLeft(patate, Canvas.GetLeft(patate) - VITESSE_JOUEUR);
                Canvas.SetLeft(salade, Canvas.GetLeft(salade) - VITESSE_JOUEUR);

                // Gestion des arrière-plans
                Canvas.SetLeft(background, Canvas.GetLeft(background) - VITESSE_JOUEUR);
                Canvas.SetLeft(background2, Canvas.GetLeft(background2) - VITESSE_JOUEUR);
                Canvas.SetLeft(background3, Canvas.GetLeft(background3) - VITESSE_JOUEUR);

                //Logique de bouclage des arrière - plans
                if (Canvas.GetLeft(background) < -background.ActualWidth) // < -1262
                {
                    Canvas.SetLeft(background, 0);
                    Canvas.SetLeft(background2, background.ActualWidth);
                    Canvas.SetLeft(background3, -background.ActualWidth);

                    //Positionne les éléments aléatoirement dans la fenêtre en fonction des listes avec les valeurs définies
                    Canvas.SetTop(fromage, positionElementY[rnd.Next(0, positionElementY.Length)]);
                    Canvas.SetTop(charcuterie, positionElementY[rnd.Next(0, positionElementY.Length)]);
                    Canvas.SetTop(patate, positionElementY[rnd.Next(0, positionElementY.Length)]);
                    Canvas.SetTop(imgPlateforme, imgPlateformePositionY[rnd.Next(0, imgPlateformePositionY.Length)]);
                    Canvas.SetTop(salade, positionElementY[rnd.Next(0, positionElementY.Length)]);
                }

                // Pour que le joueur ne sorte pas de l'écran à droite
                if (Canvas.GetLeft(joueur) > 754 - joueur.Width)
                    Canvas.SetLeft(joueur, 754 - joueur.Width);
            }

            if (goGauche)
            {
                if (Canvas.GetLeft(joueur) > 46) // Met une limite à gauche
                {
                    RunSpriteGauche(spriteIndex);
                    Run_1_Droite.Source = Run_1_Gauch;
                    Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) - VITESSE_JOUEUR);

                    Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) + VITESSE_JOUEUR);
                    Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) + VITESSE_JOUEUR);
                    Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) + VITESSE_JOUEUR);
                    Canvas.SetLeft(patate, Canvas.GetLeft(patate) + VITESSE_JOUEUR);
                    Canvas.SetLeft(salade, Canvas.GetLeft(salade) + VITESSE_JOUEUR);

                    Canvas.SetLeft(background, Canvas.GetLeft(background) + VITESSE_JOUEUR);
                    Canvas.SetLeft(background2, Canvas.GetLeft(background2) + VITESSE_JOUEUR);
                    Canvas.SetLeft(background3, Canvas.GetLeft(background3) + VITESSE_JOUEUR);

                    //Logique de bouclage des arrière - plans
                    if (Canvas.GetLeft(background3) > 0)
                    {
                        Canvas.SetLeft(background, 0);
                        Canvas.SetLeft(background2, background.ActualWidth);
                        Canvas.SetLeft(background3, -background.ActualWidth);

                        // Positionne les éléments aléatoirement dans la fenêtre en fonction des listes avec les valeurs définies
                        Canvas.SetTop(fromage, positionElementX[rnd.Next(0, positionElementX.Length)]);
                        Canvas.SetTop(charcuterie, positionElementX[rnd.Next(0, positionElementX.Length)]);
                        Canvas.SetTop(patate, positionElementX[rnd.Next(0, positionElementX.Length)]);
                        Canvas.SetTop(imgPlateforme, imgPlateformePositionY[rnd.Next(0, imgPlateformePositionY.Length)]);
                        Canvas.SetTop(salade, positionElementX[rnd.Next(0, positionElementX.Length)]);

                    }
                    // Pour pas que le joueur sorte de l'écran à gauche
                    if (Canvas.GetLeft(joueur) < 46)
                        Canvas.SetLeft(joueur, 46);
                }
            }
        }
        private void SelectionDifficulte(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedDifficulty = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            switch (selectedDifficulty)
            {
                case "Facile":
                    objectifFromages = OBJETS_FACILE;
                    objectifCharcuteries = OBJETS_FACILE;
                    objectifPatates = OBJETS_FACILE;
                    objectifSalades = OBJETS_FACILE;
                    MessageBox.Show($"Difficulté sélectionnée : {selectedDifficulty}\nCollectez {OBJETS_FACILE} de chaque élément!");
                    break;
                case "Moyen":
                    objectifFromages = OBJETS_MOYEN;
                    objectifCharcuteries = OBJETS_MOYEN;
                    objectifPatates = OBJETS_MOYEN;
                    objectifSalades = OBJETS_MOYEN;
                    MessageBox.Show($"Difficulté sélectionnée : {selectedDifficulty}\nCollectez {OBJETS_MOYEN} de chaque élément!");
                    break;
                case "Difficile":
                    objectifFromages = OBJETS_DIFFICILE;
                    objectifCharcuteries = OBJETS_DIFFICILE;
                    objectifPatates = OBJETS_DIFFICILE;
                    objectifSalades = OBJETS_DIFFICILE;
                    MessageBox.Show($"Difficulté sélectionnée : {selectedDifficulty}\nCollectez {OBJETS_DIFFICILE} de chaque élément!");
                    break;
            }
            // Mettre à jour l'affichage des compteurs avec les objectifs
            MettreAJourCompteurs();
        }


        private void MettreAJourCompteurs()
        {
            compteurFromage.Content = $"{compteurFromages}/{objectifFromages}";
            compteurCharcuterie.Content = $"{compteurCharcuteries}/{objectifCharcuteries}";
            compteurPatate.Content = $"{compteurPatates}/{objectifPatates}";
            compteurSalade.Content = $"{compteurSalades}/{objectifSalades}";
        }


        ////private void InitMusique()
        ////{
        ////    musiqueJeu = new MediaPlayer();
        ////    musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/musiquefond.mp3"));
        ////    musiqueJeu.MediaEnded += RelanceMusique;
        ////    musiqueJeu.Volume = 0.5;
        ////    musiqueJeu.Play();
        ////}

        ////private void RelanceMusique(object? sender, EventArgs e)
        ////{
        ////    musiqueJeu.Position = TimeSpan.Zero;
        ////    musiqueJeu.Play();
        ////}



    }

}

