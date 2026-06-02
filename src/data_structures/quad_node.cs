/*
 Yazarlar: Ayşenur Çiftçi
 Modul: QuadNode - Harita Tabanlı Konumsal Bolmeler
 Acıklama: QuadTree yapısının her bir dugumunu temsil eder.
 Ekran/Harita koordinat sistemine (Asagı indikce Y artar) uygun bolunme mantıgını icerir.

CORE FUNCTIONALITY & COORDINATE SYSTEM MAPPING
  Bu sınıf, QuadTree hiyerarşisinin temel uzamsal birimini (Düğüm) temsil eder.
  2D düzlem üzerindeki geometrik sınır kontrolünü (Bounding Box) ve kapasite aşımı
  durumunda alanın özyinelemeli (recursive) olarak bölünmesini yönetir.

  Mevcut tasarımda ekran ve dijital harita koordinat sistemleri (Sol üst köşe 0,0 bazlı,
  aşağı inildikçe Y ekseninin artış gösterdiği model) referans alınmıştır. Subdivide
  işlemi sırasında çeyrek merkezleri bu matris yapısına göre hesaplanır.
 */

using System.Collections.Generic;
using SmartTransit.Models;

namespace SmartTransit.DataStructures
{
    public class QuadNode
    {
        // Bölgenin merkez koordinatları (X, Y) ve yarım genişlik / yarım yükseklik değerleri
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public bool IsDivided { get; set; } = false;

        // Bu dugumun icindeki istasyonlar
        public List<Station> Stations { get; set; }

        // Dort ana alt ceyrek
        public QuadNode NorthWest { get; set; }
        public QuadNode NorthEast { get; set; }
        public QuadNode SouthWest { get; set; }
        public QuadNode SouthEast { get; set; }

        public QuadNode(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Stations = new List<Station>();
        }

        // Harita/Ekran Koordinat Sistemine Gore Subdivide (Sol ust = kucuk Y)
        public void Subdivide()
        {
            double halfWidth = Width / 2;
            double halfHeight = Height / 2;

            // Kuzey (North) bolgelerinde Y'den halfHeight cıkarılır (Yukarı eksen kucuk Y)
            // Guney (South) bolgelerinde Y'ye halfHeight eklenir (Asagı eksen buyuk Y)
            NorthWest = new QuadNode(X - halfWidth, Y - halfHeight, halfWidth, halfHeight);
            NorthEast = new QuadNode(X + halfWidth, Y - halfHeight, halfWidth, halfHeight);
            SouthWest = new QuadNode(X - halfWidth, Y + halfHeight, halfWidth, halfHeight);
            SouthEast = new QuadNode(X + halfWidth, Y + halfHeight, halfWidth, halfHeight);

            IsDivided = true;
        }

        // Istasyonun bu sınırların icinde olup olmadıgını kontrol eder
        public bool Contains(double stX, double stY)
        {
            return stX >= X - Width && stX <= X + Width &&
                   stY >= Y - Height && stY <= Y + Height;
        }
    }
}
