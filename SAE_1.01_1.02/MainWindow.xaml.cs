﻿using System;
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
        static readonly double VITESSE_JOUEUR = 3;
        static readonly double VITESSE_ENNEMI = 5;

        ///Deplacements 
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
        Rect bossHitBox;

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

        private static BitmapImage newRunner_01_Droite;
        private static BitmapImage newRunner_01_Gauche;

        // private static BitmapImage platforms;

        ImageBrush playerSprite = new ImageBrush();
        ImageBrush playerSpriteGauche = new ImageBrush();
        ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush ennemiSprite = new ImageBrush();

        // Positions à laquelle spawn aléatoirement les éléments dans une tranche donnée en Y
        int[] imgPlateformePosition = { 240, 250, 260, 270 }; 
        int[] fromagePosition = { 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320 };
        int[] charcuteriePosition = { 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320 };
        int[] patatePosition = { 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320 };

        int tempsEcoule;
        int temps;
        int compteurFromages;
        int compteurCharcuteries;
        int compteurPatates;

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

            InitMusique();

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
            if (goDroite)
            {
                // Mouvement vers la droite
                Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) + VITESSE_JOUEUR);
                RunSprite(spriteIndex);
                imgNewRunner01_Droite.Source = newRunner_01_Droite;

                // Déplacements inverses des autres éléments
                Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) - VITESSE_JOUEUR);
                Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) - VITESSE_JOUEUR);
                Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) - VITESSE_JOUEUR);
                Canvas.SetLeft(patate, Canvas.GetLeft(patate) - VITESSE_JOUEUR);

                // Gestion des arrière-plans
                Canvas.SetLeft(background, Canvas.GetLeft(background) - VITESSE_JOUEUR);
                Canvas.SetLeft(background2, Canvas.GetLeft(background2) - VITESSE_JOUEUR);
                Canvas.SetLeft(background3, Canvas.GetLeft(background3) - VITESSE_JOUEUR);

                Random random = new Random();  // Instance de Random pour générer des nombres aléatoires

                //Logique de bouclage des arrière - plans
                if (Canvas.GetLeft(background) < -background.ActualWidth)
                {
                    Canvas.SetLeft(background, 0);
                    Canvas.SetLeft(background2, background.ActualWidth);
                    Canvas.SetLeft(background3, -background.ActualWidth);

                    compteurBackground++;

                    // Positionne les éléments aléatoirement dans la fenêtre en fonction des listes avec les valeurs définies
                    Canvas.SetTop(fromage, fromagePosition[rnd.Next(0, fromagePosition.Length)]);
                    Canvas.SetTop(charcuterie, charcuteriePosition[rnd.Next(0, charcuteriePosition.Length)]);
                    Canvas.SetTop(patate, patatePosition[rnd.Next(0, patatePosition.Length)]);
                    Canvas.SetTop(imgPlateforme, imgPlateformePosition[rnd.Next(0, imgPlateformePosition.Length)]);
                }
                
                // Prevent player from moving off the right side of the screen
                if (Canvas.GetLeft(joueur) > 754 - joueur.Width)
                    Canvas.SetLeft(joueur, 754 - joueur.Width);

                double positionActuelle = Canvas.GetLeft(joueur);
                double distanceDeplacement = positionActuelle - dernierePositionX;
                distanceParcourue += Math.Abs(distanceDeplacement);
                dernierePositionX = positionActuelle;
                compteurDistance.Content = $"Distance: {Math.Round(distanceParcourue)} pixels";
            }
            if (goGauche)
            {
                if (Canvas.GetLeft(joueur) > 46) // Limite à gauche
                {
                    RunSpriteGauche(spriteIndex);
                    imgNewRunner01_Droite.Source = newRunner_01_Gauche;
                    Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) - VITESSE_JOUEUR);

                    Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) + VITESSE_JOUEUR);
                    Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) + VITESSE_JOUEUR);
                    Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) + VITESSE_JOUEUR);
                    Canvas.SetLeft(patate, Canvas.GetLeft(patate) + VITESSE_JOUEUR);

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
                        Canvas.SetTop(fromage, fromagePosition[rnd.Next(0, fromagePosition.Length)]);
                        Canvas.SetTop(charcuterie, charcuteriePosition[rnd.Next(0, charcuteriePosition.Length)]);
                        Canvas.SetTop(patate, patatePosition[rnd.Next(0, patatePosition.Length)]);
                        Canvas.SetTop(imgPlateforme, imgPlateformePosition[rnd.Next(0, imgPlateformePosition.Length)]);

                    }
                    // Prevent player from moving off the left side of the screen
                    if (Canvas.GetLeft(joueur) < 46)
                        Canvas.SetLeft(joueur, 46);

                }

                double positionActuelle = Canvas.GetLeft(joueur);
                double distanceDeplacement = positionActuelle - dernierePositionX;
                distanceParcourue -= Math.Abs(distanceDeplacement);
                dernierePositionX = positionActuelle;
                compteurDistance.Content = $"Distance: {Math.Round(distanceParcourue)} pixels";

                // Diagnostic pour vérifier la position
                Console.WriteLine($"Position Joueur: {Canvas.GetLeft(joueur)}");

                //    RunSpriteGauche(spriteIndex);
                //imgNewRunner01_Droite.Source = newRunner_01_Gauche;

                //// Move the actual player character
                //Canvas.SetLeft(joueur, Canvas.GetLeft(joueur) - VITESSE_JOUEUR);

                //// Move the actual plateforme at the same speed as the player but inversely
                //Canvas.SetLeft(imgPlateforme, Canvas.GetLeft(imgPlateforme) + VITESSE_JOUEUR);

                //// Move the actual fromage at the same speed as the player but inversely
                //Canvas.SetLeft(fromage, Canvas.GetLeft(fromage) + VITESSE_JOUEUR);

                //// Move the actual charcuterie at the same speed as the player but inversely
                //Canvas.SetLeft(charcuterie, Canvas.GetLeft(charcuterie) + VITESSE_JOUEUR);

                //// Move the actual patate at the same speed as the player but inversely
                //Canvas.SetLeft(patate, Canvas.GetLeft(patate) + VITESSE_JOUEUR);



                //// FOR THE BACKGROUND TO MOVE 
                //Canvas.SetLeft(background, Canvas.GetLeft(background) + VITESSE_JOUEUR); // Vitesse
                //Canvas.SetLeft(background3, Canvas.GetLeft(background3) + VITESSE_JOUEUR); // Vitesse
                //Canvas.SetLeft(background2, Canvas.GetLeft(background2) + VITESSE_JOUEUR); // Vitesse


                // Loop the backgrounds when it goes off-screen



            }

            // When launching the game, the player goes down to hit the ground
            Canvas.SetTop(joueur, Canvas.GetTop(joueur) + speed);

            // moves the ennemi horizontally to the left by 5 units.
            Canvas.SetLeft(ennemi, Canvas.GetLeft(ennemi) - VITESSE_ENNEMI);

            tempsText.Content = "Temps : " + temps;

            // Setup 3 Hitboxs
            joueurHitBox = new Rect(Canvas.GetLeft(joueur), Canvas.GetTop(joueur), joueur.Width - 15, joueur.Height);
            ennemiHitBox = new Rect(Canvas.GetLeft(ennemi), Canvas.GetTop(ennemi), ennemi.Width, ennemi.Height);
            solHitBox = new Rect(Canvas.GetLeft(sol), Canvas.GetTop(sol), sol.Width, sol.Height);
            platformsHitBox = new Rect(Canvas.GetLeft(imgPlateforme), Canvas.GetTop(imgPlateforme), imgPlateforme.Width, imgPlateforme.Height);
            fromageHitBox = new Rect(Canvas.GetLeft(fromage), Canvas.GetTop(fromage), fromage.Width, fromage.Height);
            charcuterieHitBox = new Rect(Canvas.GetLeft(charcuterie), Canvas.GetTop(charcuterie), charcuterie.Width, charcuterie.Height);
            patateHitBox = new Rect(Canvas.GetLeft(patate), Canvas.GetTop(patate), patate.Width, patate.Height);
            bossHitBox = new Rect(Canvas.GetLeft(boss), Canvas.GetTop(boss), boss.Width, boss.Height);

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

            if (joueurHitBox.IntersectsWith(patateHitBox))
            {
                // On remet la patate à sa position initiale
                Canvas.SetLeft(patate, 1350);
                Canvas.SetTop(patate, 200);
                compteurPatates += 1;
                compteurPatate.Content = compteurPatates;
            }
            if (compteurBackground >= 5)
            {
                boss.Opacity = 100;
                Canvas.SetLeft(boss, 650);
                Canvas.SetTop(boss, 321);
                Console.WriteLine($"Boss spawning. Background counter : {compteurBackground}");
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
                compteurPatates = 0;
                tempsText.Content = "Temps : " + temps + " \nAppuyer entrer pour rejouer !";
                compteurFromage.Content = compteurFromages;
                compteurCharcuterie.Content = compteurCharcuteries;
                compteurPatate.Content = compteurPatates;
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
                if (Canvas.GetLeft(joueur) > 0) // Vérification de la limite gauche
                {
                    goGauche = true;
                }
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
            Canvas.SetTop(imgPlateforme, imgPlateformePosition[rnd.Next(0, imgPlateformePosition.Length)]);

            // Set the fromage 
            Canvas.SetLeft(fromage, 200); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(fromage) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(fromage, fromagePosition[rnd.Next(0, fromagePosition.Length)]);

            // Set the charcuterie 
            Canvas.SetLeft(charcuterie, 300); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(charcuterie) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(charcuterie, charcuteriePosition[rnd.Next(0, charcuteriePosition.Length)]);

            // Set the patate 
            Canvas.SetLeft(patate, 400); // Obliger de mettre des coordonnées car si j'utilise Canvas.GetLeft(charcuterie) et Canvas.GetTop quand le joueur touche un obstacle ca reset pas
            Canvas.SetTop(patate, patatePosition[rnd.Next(0, patatePosition.Length)]);

            // Set the Boss
            Canvas.SetLeft(boss, 700);
            Canvas.SetTop(boss, 200);
            boss.Opacity = 0;
            RunSprite(1);

            ennemiSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/ennemi_1.png"));
            ennemi.Fill = ennemiSprite;

            saut = false;
            gameOver = false;
            tempsEcoule = 0;
            temps = 180;
            compteurFromage.Content = 0;
            compteurCharcuterie.Content = 0;
            compteurPatate.Content = 0;

            distanceParcourue = 0;
            dernierePositionX = Canvas.GetLeft(joueur);
            compteurDistance.Content = distanceParcourue;

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

        protected override void OnKeyDown(KeyEventArgs e) // pour acceder a la page en appuyant sur une touche
        {

            if (e.Key == Key.Escape)
            {
                // Ouvrir le menu
                WindowMenuEchap windowMenuEchap = new WindowMenuEchap();
                windowMenuEchap.ShowDialog();
            }
        }

        private void InitMusique()
        {
            musiqueJeu = new MediaPlayer();
            musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/musiquefond.mp3"));
            musiqueJeu.MediaEnded += RelanceMusique;
            musiqueJeu.Volume = 0.5;
            musiqueJeu.Play();
        }

        private void RelanceMusique(object? sender, EventArgs e)
        {
            musiqueJeu.Position = TimeSpan.Zero;
            musiqueJeu.Play();
        }

        //private void DeplacementEnnemi()
        //{
        //    // Déplacement horizontal du chien
        //    double nouvellePositionX = Canvas.GetLeft(ennemiFinal) + Math.Sign(Canvas.GetLeft(joueur) - Canvas.GetLeft(ennemiFinal));
        //    if (nouvellePositionX >= 0 && nouvellePositionX + ennemiFinal.Width <= background.ActualWidth)
        //    {
        //        Canvas.SetLeft(ennemiFinal, nouvellePositionX);

        //    }

        //    // Déplacement vertical du chien
        //    double nouvellePositionY = Canvas.GetTop(ennemiFinal) + Math.Sign(Canvas.GetTop(joueur) - Canvas.GetTop(ennemiFinal));
        //    if (nouvellePositionY >= 0 && nouvellePositionY + ennemiFinal.Height <= background.ActualHeight)
        //    {
        //        Canvas.SetTop(ennemiFinal, nouvellePositionY);

        //    }


        //}

    }
}

