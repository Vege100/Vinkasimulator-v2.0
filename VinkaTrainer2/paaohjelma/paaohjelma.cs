using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace paaohjelma
{
    public class paaohjelma : PhysicsGame
    {
        PhysicsObject pelaaja;
        IntMeter pisteLaskuri;
        int raha = 30;
        int elama = 3;
        int telama = 3;
        double ohjusnopeus = 2;
        
        public override void Begin()
        {
            IsFullScreen = true;
            Valikko();
        }
        
        void Valikko()
        {
            ClearAll();
            MultiSelectWindow alkuValikko = new MultiSelectWindow("VinkaTrainer2", "Start", "Points", "Shop", "Quit");
            Add(alkuValikko);


            alkuValikko.AddItemHandler(0, Start);
            alkuValikko.AddItemHandler(1, Points);
            alkuValikko.AddItemHandler(2, Shop);
            alkuValikko.AddItemHandler(3, Exit);
        }
        void Pausevalikko()
        {
            MultiSelectWindow pausevalikko = new MultiSelectWindow("", "Continue", "Giveup");
            Pause();
            Add(pausevalikko);
            pausevalikko.AddItemHandler(0, Pause);
            pausevalikko.AddItemHandler(1, Gameover);

        }
        void Start()
        {   
            Luokentta();
            Asetaohjaimet();
            LuoPistelaskuri();
            Timer.SingleShot(1,AmmuOhjus);
            Timer.CreateAndStart(2, Ammukolikko);

        }
        void Points()
        {
            MultiSelectWindow points = new MultiSelectWindow("", "Back");
            Add(points);
            points.AddItemHandler(0, Valikko);
        }
        void Shop()
        {
            ClearAll();
            MultiSelectWindow shop = new MultiSelectWindow("", "Back", "Life + 1","Casino");
            Add(shop);
            shop.AddItemHandler(0, Valikko);
            shop.AddItemHandler(1, Elamaa);
            shop.AddItemHandler(2, Casino);
            Label rahana = new Label(50, 20, raha.ToString());
            rahana.Position = new Vector(Level.Right - 100, Level.Top - 100);
            Add(rahana);
        }
        void Elamaa()
        {
            if (raha >= 10)
            {
             telama += 1;
             raha -= 10;
            }
            Shop();
        }
        void Casino()
        {
            MultiSelectWindow casino = new MultiSelectWindow("", "SPIN");
            casino.AddItemHandler(0, Spinneri);
            Add(casino);

        }
        void Spinneri()
        {
            for (int i = 0; i < 10; i++)
            {
                int random = RandomGen.NextInt(0, 2);
                Label piir = new Label(20,20, random.ToString());
                piir.Position = new Vector(Level.Left + 100 + i * 100, Level.Top - 100);
                Add(piir);
            }
            Casino();
            
        }
        public static string[] Etsisaa()
        {
            string[] mika = new string[3];
            string saa = Lataanetista("https://www.getmetar.com/EFJY");
            int a = saa.IndexOf("> EFJY ");
            int b = saa.IndexOf(" Q");
            saa = saa.Substring(a, b - a);
            mika[0] = Etsipilvet(saa);
            mika[1] = Etsisade(saa);
            return mika;
        }
        public static string Etsipilvet(string saa)
        {
            string pilvi = "";
            string[] pilvet = { "SKC", "FEW", "SCT", "BKN", "OVC"};
            foreach (string s in pilvet)
            {
                if (saa.Contains(s))
                {
                    pilvi = s;
                }
            }
            return pilvi;
        }
        public static string Etsisade(string saa)
        {
            string sade = "";
            string[] sateet = { "DZ", "GR", "GS", "IC", "PL", "RA", "SG", "SN" };
            foreach (string s in sateet)
            {
                if (saa.Contains(s))
                {
                    sade = "RA";
                }
            }
            return sade;
        }
        public static double Etsiaika()
        {
            DateTime aika = DateTime.Now;
            int paiva = aika.Day;
            int kuukausi = aika.Month;
            int vuosi = aika.Year;
            string kulma = Lataanetista("https://www.timeanddate.com/sun/finland/jyvaskyla");
            int a = kulma.IndexOf("sunalt");
            kulma = kulma.Substring(a + 7, 5);
            string[] numero = kulma.Split(',');
            double akulma = Convert.ToDouble(numero[0] + numero[1][0]);
            akulma = 0;
            return akulma;
        }
        public static string Lataanetista(string osoite)
        {
            WebClient client = new WebClient();
            string lataus = client.DownloadString(osoite);
            return lataus;
        }
        void Luosaa()
        {
            string[] saa = Etsisaa();
            if (saa[0].Length != 0)
            {
                GameObject pilvi = new(Screen.Width, Screen.Height);
                pilvi.Image = LoadImage(saa[0]);
                Add(pilvi,1);
            }
            if (saa[1].Length != 0)
            {
                GameObject sade = new(Screen.Width, Screen.Height);
            }
        }
        void Piirraaurinko(double ak)
        {
            GameObject aurinko = new(200, 200);
            aurinko.Shape = Shape.Circle;
            aurinko.Color = new Jypeli.Color(1, (100+ak)/510, 0);
            aurinko.Position = new Vector(0, Level.Bottom+10*(ak+65));
            Add(aurinko, -1);
            
        }
        void Piirrataivas(double ak)
        {
            Level.Background.CreateStars(1000);
            GameObject tummennus = new(Screen.Width, Screen.Height);
            tummennus.Color = new Color(0, 0, 0, 135);
            Add(tummennus, 2);
        }
        
        void Luokentta()
        {
            Luosaa();
            double auringonkulma = Etsiaika();
            if (auringonkulma > -65) Piirraaurinko(auringonkulma);
            else Piirrataivas(auringonkulma);
            pelaaja = LuoPelaaja();
            AddCollisionHandler(pelaaja, "ohjus", Pelaajatormasi);
            AddCollisionHandler(pelaaja, "kolikko", Pisteita);
            ohjusnopeus = 2;
            elama = telama;

            Level.Height = Screen.Height;
            Level.Width = Screen.Width;



        }
        void Pisteita(PhysicsObject a, PhysicsObject b)
        {
            raha += 1;
            b.Destroy();
        }
        void Pelaajatormasi(PhysicsObject pelaaja, PhysicsObject ohjus)
        {
            elama -= 1;
            Explosion rajahdys = new Explosion(ohjus.Width);
            rajahdys.Position = (ohjus.Position);
            Add(rajahdys);
            ohjus.Destroy();
            if (elama <= 0)
            {
                Explosion pommi = new Explosion(ohjus.Width);
                pommi.Position = (pelaaja.Position);
                Add(pommi);
                Add(rajahdys);
                pelaaja.Destroy();
                Timer.SingleShot(2, Gameover);
            }
        }
        void Gameover()
        {
            int loppupisteet = pisteLaskuri.Value;
            ClearAll();
            MultiSelectWindow loppu = new MultiSelectWindow("kuolit", "Continue");
            Add(loppu);
            loppu.AddItemHandler(0, Valikko);
            loppu.Position = new Vector(0, -200);
            Label pisteita = new Label(500, 20, $"Points: {loppupisteet}");
            pisteita.Position = new Vector(0, 0);
            Label rahaa = new Label(500, 20, $"Cash: {raha}");
            rahaa.Position = new Vector(0, -50);
            Add(pisteita);
            Add(rahaa);
        }
        PhysicsObject LuoPelaaja()
        {
            Image vinka = LoadImage("vinkafreimi");
            PhysicsObject pelaaja = PhysicsObject.CreateStaticObject(333, 120);
            pelaaja.Shape = Shape.FromImage(vinka);
            pelaaja.Y = 0;
            pelaaja.Tag = "pelaaja";
            pelaaja.X = Level.Left + 150;
            pelaaja.Image = LoadImage("vinka3");
            Add(pelaaja,0);
            return pelaaja;

        }

        void AmmuOhjus()
        {
            PhysicsObject ohjus;
            ohjus = new PhysicsObject(105, 25);
            int y = RandomGen.NextInt(-500, 500);
            ohjus.Y = y;
            int nopeus = -400;
            ohjus.X = Level.Right;
            int kuva = RandomGen.NextInt(0, 7);
            string[] t = { "ohjus1", "ohjus2", "ohjus3", "ohjus4", "ohjus5", "ohjus6", "ohjus7" };
            Image ohjuskuva = LoadImage(t[kuva]);
            ohjus.Shape = Shape.FromImage(ohjuskuva);
            ohjus.Image = ohjuskuva;
            ohjus.Tag = "ohjus";
            ohjus.LifetimeLeft = new (50000000);
            pisteLaskuri.Value += 1;
            if (ohjusnopeus > 0.4) ohjusnopeus = (ohjusnopeus * 0.97);
            else nopeus = pisteLaskuri.Value * -7;
            ohjus.Velocity = new Vector(nopeus, 0);
            Add(ohjus);
            Timer.SingleShot(ohjusnopeus, AmmuOhjus);

        }
        void Ammukolikko()
        {

            int tod = RandomGen.NextInt(0, 10);
            if (tod < 5) return;
            PhysicsObject kolikko;
            kolikko = new PhysicsObject(50, 50);
            kolikko.Shape = Shape.Circle;
            int y = RandomGen.NextInt(-500, 500);
            kolikko.Y = y;
            kolikko.Tag = "kolikko";
            kolikko.X = Level.Right;
            kolikko.Image = LoadImage("kolikko");
            kolikko.Velocity = new Vector(-400, 0);
            Add(kolikko);


        }
        void LuoPistelaskuri()
        {
            pisteLaskuri = new IntMeter(0);

            Label pisteNaytto = new Label();
            pisteNaytto.X = Screen.Left + 100;
            pisteNaytto.Y = Screen.Top - 100;
            pisteNaytto.TextColor = Color.Black;
            pisteNaytto.Color = Color.White;

            pisteNaytto.BindTo(pisteLaskuri);
            Add(pisteNaytto);
        }
        void Asetaohjaimet()
        {
            Vector nopeusYlos = new Vector(0, 500);
            Vector nopeusAlas = new Vector(0, -500);

            Keyboard.Listen(Key.W, ButtonState.Down, AsetaNopeus, "Kaarto ylös", pelaaja, nopeusYlos);
            Keyboard.Listen(Key.W, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
            Keyboard.Listen(Key.S, ButtonState.Down, AsetaNopeus, "Kaarto alas", pelaaja, nopeusAlas);
            Keyboard.Listen(Key.S, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);

            Keyboard.Listen(Key.Up, ButtonState.Down, AsetaNopeus, "Kaarto ylös", pelaaja, nopeusYlos);
            Keyboard.Listen(Key.Up, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
            Keyboard.Listen(Key.Down, ButtonState.Down, AsetaNopeus, "Kaarto alas", pelaaja, nopeusAlas);
            Keyboard.Listen(Key.Down, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);

            Keyboard.Listen(Key.M, ButtonState.Pressed, Pausevalikko, "pause");
        }

        void AsetaNopeus(PhysicsObject kone, Vector nopeus)
        {
            if ((nopeus.Y < 0) && (kone.Bottom < Level.Bottom))
            {
                kone.Velocity = Vector.Zero;
                return;
            }
            if ((nopeus.Y > 0) && (kone.Top > Level.Top))
            {
                kone.Velocity = Vector.Zero;
                return;
            }

            kone.Velocity = nopeus;
            if (nopeus.Y > 0) kone.Angle = Angle.FromDegrees(15);
            if (nopeus.Y < 0) kone.Angle = Angle.FromDegrees(-15);
            if (nopeus.Y == 0) kone.Angle = Angle.FromDegrees(0);
        }
    }
        
}