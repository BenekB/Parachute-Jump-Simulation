using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;

namespace MMM
{
    public partial class Form1 : Form           //w tej klasie znajdują się wszystkie zdarzenia, czyli
                                                //jak ktoś coś klinkie w formularzu, to tu jest punkt wyjścia
    {

        public Form1()                         //to jest konstruktor klasy - to co tam jest wykonuje się w momencie włączenia aplikacji
        {
            InitializeComponent();
        }


        Obliczenia obliczenia;

        int h = 0;      //zmienna, która będzie potrzebna do wypisywania odpowiednich próbek na ekranie


        public void button1_Click(object sender, EventArgs e)       //dzieje się, gdy przycisk numer 1 zostanie naciśnięty
                                                                    //chodzi o przycisk START
        {
            obliczenia = new Obliczenia(double.Parse(textBox1.Text), double.Parse(textBox2.Text), 
                                                   double.Parse(textBox3.Text));
            //powyżej tworzę instancję klasy Obliczenia i od razu zapisuję do niej wartości z tych trzech okienek do wpisywania
            //w formularzu

            obliczenia.equation();      //rozwiązuje równanie różniczkowe, a wyniki wrzuca do wektorów 

            h = 0;          //zeruję indeks próbki, żeby zacząć wypisywanie od początku

            timer1.Start();         //włączam timer, który będzie wywoływał funkcję do wypisywania wartości chwilowych 
                                    //prędkości, przyspieszenia i odległości co 250 milisekund
                                    
        }


        public void pisz_wartosci(double y_0_d, double y_1_d, double y_2_d)          //wypisuje wartości zadane w ćwiczeniu na ekranie
        {
            //normalnie liczę drogę jaką przebyło ciało, a chcę wyświetlać wysokość na jakiej się znajduje,
            //więc poniżej odejmuję od wysokości początkowej przebytą przez ciało drogę
            y_0_d = obliczenia.y - y_0_d;           

            //zaokrąglam do dwóch miejsc po przecinku wszystkie wartości
            y_0_d = y_0_d - y_0_d % 0.01;
            y_1_d = y_1_d - y_1_d % 0.01;
            y_2_d = y_2_d - y_2_d % 0.01;

            //i zamieniam na stringi do wyświetlenia na ekranie
            string y_0 = y_0_d.ToString();
            string y_1 = y_1_d.ToString();
            string y_2 = y_2_d.ToString();

            //dodaję jednostki do wartości
            y_0 += " m";
            y_1 += " m/s";
            y_2 += " m/(s^2)";

            Graphics graphics = CreateGraphics();                                   //standard przy tworzeniu grafiki 
            Font font = new Font("Arial", 16);                                      //ustawianie czcionki
            SolidBrush solidBrush = new SolidBrush(Color.Black);                    //ustawianie pędzla

            graphics.Clear(Color.White);                                            
            graphics.DrawString(y_0, font, solidBrush, 700, 120);                  //no i tu są konkretne polecenia do wypisywania tekstu
            graphics.DrawString(y_1, font, solidBrush, 700, 165);                          
            graphics.DrawString(y_2, font, solidBrush, 700, 207);              

            //poniższe instrukcje warunkowe działają w ten sposób, że w czasie działania programu wyświetlają
            //pełne sekundy, a po zakończeniu dokładny czas spadania z dokładnością do tysięcznej części sekundy
            if (h == obliczenia.y_0_queue.Count - 1)                                
                graphics.DrawString((h / 1000.0).ToString() + " s", font, solidBrush, 700, 80);
            else
                graphics.DrawString((h / 1000).ToString() + " s", font, solidBrush, 700, 80);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (obliczenia.y_0_queue.Count > h)      //wykonuje się dopóki ilość elementów wektora jest mniejsza niż indeks 
                                                     //wartości, którą chcemy pobrać - żeby nie wyjść poza zakres wektora
            {
                pisz_wartosci((double)obliczenia.y_0_queue[h], (double)obliczenia.y_1_queue[h], (double)obliczenia.y_2_queue[h]);
                //powyżej przekazujemy do funkcji pisz_wartosci wartości poszczególnych parametrów (położenie, prędkość, przyspieszenie)
                //o indeksie h

                h += 250;       //zmieniamy h co 250, bo co tyle wyświetlam na ekranie aktualną wartość, a symulacja jest z krokiem co 
                                //1 milisekundę, więc po prostu omijam 249 próbek
                                //moim zdaniem nie ma co częściej zmieniać tej wartości, bo jeżeli będzie częściej zmieniana, to nawet nie będziemy
                                //w stanie jej przeczytać
            }
            else
            {
                h = obliczenia.y_0_queue.Count - 1;       //zmienna, która przechowuje indeks ostatniej próbki

                pisz_wartosci((double)obliczenia.y_0_queue[h], (double)obliczenia.y_1_queue[h], (double)obliczenia.y_2_queue[h]);
                //powyższa funkcja wypisuje ostatnią wartość z wektora, czyli wartości w momencie uderzenia o ziemię
                //ma to sens, ponieważ pętla while kończy jeżeli liczba elementów jest większa od h
                //gdyby h było równe 1000, a wektro miałby 999 elementów, to pętla while jako ostatni wyświetli wynik o 250 próbek 
                //wcześniejszy - dlatego powyżej wyświetlam ostatni wynik przed uderzeniem o ziemię

                timer1.Stop();      //zatrzymuję timer, bo już skończył wypisywanie wszystkich wartości

                rysuj_wykres();
            }
            
        }

        
        private void rysuj_wykres ()
        {
            //pobieram całkowity czas spadania obiektu
            double czas_odniesienia = obliczenia.getTime();

            Graphics graphics = CreateGraphics();
            Pen pen = new Pen(Color.Black);
            Point start = new Point();                  //tworzę dwa punkty, które będą potrzebne do rysowania wykresów
            Point stop = new Point();
          
            //trzy tablice double w których za chwilę zapiszę wartości, z których będę korzystał przy rysowaniu wykresów
            double[] wysokosc = new double[150];
            double[] predkosc = new double[150];
            double[] przyspieszenie = new double[150];

            //w tej pętli wypełniam tablice, które znajdują się powyżej
            for (int i = 0; i < 150; i++)
            {
                //dzielę czas odniesienia na 300 równych kawałków, bo będę rysował wykres o szerokości 300 pikseli
                //i pobieram z wektora wartości w tych odstępach czasu
                //służy to skalowaniu wykresu, aby zawsze miał takie same wymiary niezależnie od rzeczywistych wartości
                wysokosc[i] = (double)obliczenia.y_0_queue[(int)(2 * i * czas_odniesienia * 1000 / 300)];
                predkosc[i] = (double)obliczenia.y_1_queue[(int)(2 * i * czas_odniesienia * 1000 / 300)];
                przyspieszenie[i] = (double)obliczenia.y_2_queue[(int)(2 * i * czas_odniesienia * 1000 / 300)];
            }

            //zastosowałem skale z tego samego powodu co powyżej, tylko tutaj są potrzebne do skalowania w pionie
            //krótko mówiąc dzielę 200 pikseli (wysokość wykresu) przez największą wartość danej wielkości
            double skala_wysokosci = 200/ wysokosc[149];
            double skala_predkosci = 200 / predkosc[149];
            double skala_przyspieszenia = 200 / przyspieszenie[0];

            //ta pentla służy do rysowania tych wykresów
            for (int i = 0; i < 149; i++)
            {
                //wysokość
                //ustalam dwa punkty między którymi będę rysował linie
                //stałe wartości służą odpowiedniemu przesunięciu wykresów
                start.X = 100 + 2*i;
                //mnożę wartość wysokości w danej chwili czasu i mnożę przez skalę tej wielkości, żeby wykres miał określoną wysokość
                start.Y = 500 + (int)(skala_wysokosci*wysokosc[i]);
                stop.X = 102 + 2 * i;
                //a tutaj to samo, tylko biorę wartość z następnej chwili czasu
                stop.Y = 500 + (int)(skala_wysokosci * wysokosc[i + 1]);

                //zastosowałem try/catch, bo przy krótkim czasie opadania przedmiotu występowały błędy
                //wykresy nie są obowiązkową częścią projektu, a raczej czymś dodatkowym, więc nie szukałem
                //o co chodzi z tym błędem. Po prostu dla małych wartości czasu opadania coś nie działało i żeby
                //program dalej działał zastosowałem try/catch 
                try
                {
                    graphics.DrawLine(pen, start, stop);
                }
                catch (Exception) { }

                //prędkość
                start.X = 500 + 2 * i;
                start.Y = 700 - (int)(skala_predkosci * predkosc[i]);
                stop.X = 502 + 2 * i;
                stop.Y = 700 - (int)(skala_predkosci * predkosc[i + 1]);

                try
                {
                    graphics.DrawLine(pen, start, stop);
                }
                catch (Exception) { }

                //przyspieszenie
                start.X = 900 + 2 * i;
                start.Y = 700 - (int)(skala_przyspieszenia * przyspieszenie[i]);
                stop.X = 902 + 2 * i;
                stop.Y = 700 - (int)(skala_przyspieszenia * przyspieszenie[i + 1]);

                try
                {
                    graphics.DrawLine(pen, start, stop);
                }
                catch (Exception) { }
            }

            //poniższe kilka linijek służy do rysowania osi dla wykresów
            Pen pen_2 = new Pen(Color.Blue);

            graphics.DrawLine(pen_2, new Point(100, 720), new Point(100, 480));
            graphics.DrawLine(pen_2, new Point(80, 700), new Point(420, 700));

            graphics.DrawLine(pen_2, new Point(500, 720), new Point(500, 480));
            graphics.DrawLine(pen_2, new Point(80 + 400, 700), new Point(420 + 400, 700));

            graphics.DrawLine(pen_2, new Point(900, 720), new Point(900, 480));
            graphics.DrawLine(pen_2, new Point(80 + 800, 700), new Point(420 + 800, 700));

        }
    }




    public class Obliczenia
    {
        public double y_2 = 0.0;                                //przyspieszenie
        public double y_1 = 0.0;                                //prędkość
        public double y_0 = 0.0;                                //położenie

        public ArrayList y_2_queue = new ArrayList();           //wektor kolejnych wartości przyspieszenia i tak dalej
        public ArrayList y_1_queue = new ArrayList();
        public ArrayList y_0_queue = new ArrayList();

        private double g = 9.81;                                //przyspieszenie ziemskie
        public double m;                                        //masa opadającego przedmiotu
        private double h = 0.001;                               //krok symulacji
        private double b;                                       //opór powietrza
        public double y;                                        //położenie początkowe


        public Obliczenia(double m, double b, double y)         //konstruktor klasy
        {
            this.m = m;                     //nadanie początkowych wartości poszczególnym zmiennym
            this.b = b;
            this.y = y;

            y_1_queue.Add(y_1);             //dodanie pierwszej wartości prędkości jako zerowej
            y_0_queue.Add(y_0);             //dodanie pierwszej wartości położenia jako zerowej
        }


        public void equation()              //tutaj jest całe równanie różniczkowe
        {

            while (y_0 < y)                 //dopóki odległość jaką przeleciał przedmiot jest mniejsza niż wysokość, z której został zrzucony
            {
                y_2 = g - (b / m) * y_1 * y_1;              //tu jest całe równanie - to co pokazywał dr Kozłowski
                y_1 = y_1 + h * y_2;
                y_0 = y_0 + h * y_1 + 0.5 * h * h * y_2;

                y_2_queue.Add(y_2);                         //dodaję kolejne wartości a, v i położenia do kolejek
                y_1_queue.Add(y_1);
                y_0_queue.Add(y_0);

            }

            y_2_queue.Add(y_2 = g - (b / m) * y_1 * y_1);       //ponieważ dodałem do kolejek położenia i prędkości wartości początkowe,
                                                                //to teraz wyrównuję liczbę elementów w kolejce przyspieszenia dodając
                                                                //przyspieszenie w następnej chwili czasu
            
        }


        //funkja potrzebna do rysowania wykresów, ponieważ potrzebuję tam całkowitego czasu opadania, żeby
        //odpowiednio skalować wykresy
        public double getTime()
        {
            double czas = y_0_queue.Count/1000;
            return czas;
        }

    }

}
