# Flappy Bird

Unity 6 ile geliştirilmiş, klasik Flappy Bird oyununun 2D klonu. Kuşu boruların arasından geçirerek puan topla; boruya ya da zemine çarparsan oyun biter.

## Oynanış

- **Zıpla:** Boşluk tuşu, sol fare tıklaması veya ekrana dokunma
- Oyun, başlangıç ekranında ilk dokunuşa kadar bekler (kuş yerinde süzülür).
- Her boru çiftinin arasından geçiş **1 puan** kazandırır.
- Oyun bittiğinde skor ve **en iyi skor** gösterilir; en iyi skor cihazda kalıcı olarak saklanır (`PlayerPrefs`).
- **Tekrar Oyna** butonu sahneyi yeniden başlatır.

## Özellikler

- Hıza göre yukarı/aşağı dönen kuş animasyonu
- Rastgele yükseklikte boru üretimi
- Object Pooling ile boru yönetimi (Instantiate/Destroy yerine; mobilde GC takılmalarını önler)
- Sonsuz kayan zemin
- Hazır / Oynanıyor / Bitti durumlarını yöneten oyun yöneticisi
- Hem yeni Input System hem de eski Input Manager desteği
- Dikey (portrait) ekran ve farklı çözünürlüklere uyumlu arayüz (TextMesh Pro)

## Gereksinimler

- **Unity 6000.3.10f1** (Unity 6)
- Universal Render Pipeline (2D)
- Input System, TextMesh Pro (proje paketlerinde tanımlı)

## Kurulum ve Çalıştırma

1. Repoyu klonla:
   ```bash
   git clone https://github.com/MetinKizil/Flappy-Bird.git
   ```
2. Unity Hub'da **Add → Add project from disk** ile klasörü aç.
3. `Assets/Scenes/Game.unity` sahnesini aç ve **Play**'e bas.

### Sahneyi sıfırdan kurmak

Sahne, sprite'lar, boru prefab'ı ve arayüz bir editör script'i ile otomatik oluşturulabilir:

- Menü: **Tools → Flappy Bird → Sahneyi Kur**
- Komut satırı:
  ```bash
  Unity.exe -batchmode -projectPath . -executeMethod FlappySceneBuilder.Build -quit
  ```

TextMesh Pro kaynakları eksikse önce **Tools → Flappy Bird → TMP Essentials Yükle** çalıştırılmalıdır.

## Proje Yapısı

```
Assets/
├── Editor/
│   └── FlappySceneBuilder.cs   # Sahneyi, sprite'ları, prefab'ı ve UI'yi tek tıkla kurar
├── Prefabs/
│   └── PipePair.prefab         # Üst/alt boru + puan bölgesi
├── Scenes/
│   └── Game.unity              # Oyun sahnesi
├── Scripts/
│   ├── Bird.cs                 # Zıplama, dönüş, girdi ve çarpışma
│   ├── GameManager.cs          # Oyun durumu, skor, en iyi skor ve UI panelleri
│   ├── PipeSpawner.cs          # Boru üretimi ve nesne havuzu
│   ├── PipePair.cs             # Boru hareketi ve havuza geri dönüş
│   ├── ScoreZone.cs            # Borular arasından geçişte puan verir
│   └── GroundScroller.cs       # Sonsuz kayan zemin
└── Sprites/                    # Kod ile üretilmiş piksel sprite'lar
```
