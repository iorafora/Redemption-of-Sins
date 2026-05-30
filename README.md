# Redemption of Sins

**Redemption of Sins**, Unity 6 motoru ve C# programlama dili kullanılarak piksel sanat (pixel art) tarzında geliştirilmiş, karanlık fantastik ve orta çağ temalı bir 2D aksiyon/macera oyunudur. Proje, modüler kod yapısı, optimize edilmiş sistemleri ve "Hissizlik (Numb)" konseptini hem hikayeye hem de mekaniklere işleyen özgün yapısı ile profesyonel bir oyun tasarımı portfolyosu sunmaktadır.

---

## 🎮 Oyunu İndir ve Oyna

Oyunun derlenmiş, optimize edilmiş ve oynanabilir son sürümünü (.exe) aşağıdaki bağlantıdan güvenli bir şekilde indirebilirsiniz:

👉 **[Redemption of Sins - Oyunu İndir (OneDrive)](👉 <a href="https://drive.google.com/drive/folders/1tRMC6H5rZ4ijPejy8Or9-HO-0yM4l_Vr?usp=sharing" target="_blank"><b>Redemption of Sins - Oyunu İndir (Google Drive)</b></a>)**

> 💡 **Not:** İndirdiğiniz `.zip` dosyasını sağ tıklayıp bir klasöre çıkardıktan sonra, klasör içerisindeki `.exe` dosyasına çift tıklayarak oyunu başlatabilirsiniz.

---

## 📖 Hikaye ve Evren Motivasyonu

Bir Tanrı'nın inananları tarafından öldürülüşü, dünyanın dengesini geri dönülmez şekilde bozmuştur. Tanrı'nın bedeninden akan kan nehirleri kirletirken, insanlığın açgözlülüğü sonucu ortaya çıkan *His Laneti*, insanları iki uç noktaya sürüklemiştir: Çoğu insan dünyayı delirtecek seviyede aşırı yoğun algılarken, küçük bir azınlık ise tamamen hissizleşmiştir.

Vücudunun bir kısmı taşlaşmış ve duygularını kaybetmiş ana karakterimiz, efsanevi Yedi Aziz'in hac yolculuğuna dayanarak kendi günahları, korkuları ve kaderiyle yüzleşmek üzere **Kefaret Yolculuğu**'na çıkar. Amacı; yeniden hissedebilmek, kaybettiklerine anlam verebilmek veya Tanrılara tüm bu acıların nedenini sorabilmektir.

---

## 🛠️ Teknik Özellikler & Geliştirilen Sistemler

Proje geliştirilirken sürdürülebilir, performans dostu ve veri odaklı yazılım prensipleri ön planda tutulmuştur:

* **Unity 6 & C# Altyapısı:** En güncel Unity sürümünün getirdiği performans avantajları ve modern C# pratikleri kullanılmıştır.
* **IL2CPP Scripting Backend:** Oyunun tüm C# kodları derleme aşamasında C++ mimarisine ve ardından makine diline dönüştürülmüştür. Bu sayede tersine mühendislik (decompile) engellenerek maksimum kod güvenliği sağlanmıştır.
* **Hissizleşme Mekaniği (Ekran Grileşmesi):** Oyun "Numb" teması üzerine kuruludur. Karakter hasar aldıkça ruhu aşınır, yavaşça taşlaşır ve mekaniksel olarak **ekran dinamik şekilde grileşmeye başlar**; belirli bir düzey geçildiğinde ise karakter ölür.
* **Modüler Diyalog ve Seçim Sistemi:** NPC'ler ile konuşma, sunulan seçeneklere göre farklı tepkiler (Dinamik NPC Response) alma sistemini yöneten esnek ve genişletilebilir bir diyalog mimarisi kodlanmıştır.
* **Gelişmiş Boss Yapay Zekası (Fireguy AI):** Durum makineleri (State Machine) kullanılarak boss'un can durumuna ve oyuncunun mesafesine göre dinamik kararlar alabilen yapay zeka mekanikleri tasarlanmıştır.
* **Sahne ve Sinematik Yönetimi (SceneIntroManager):** Sahne açılışlarındaki kararmalar, oyuncu kontrollerini dinamik olarak kilitleme/açma (`LockPlayer`) ve boss ölüm sekanslarının ardından sorunsuz bir şekilde `MainMenu` sahnesine geçiş sağlayan asenkron korutin (Coroutine) tabanlı bir yönetim sistemi kurulmuştur.
* **Gelişmiş Kontrol ve Kısayollar:** Akıcı bir seyahat ve parkur deneyimi için anlık dokunulmazlık sağlayan **Dash Sistemi** entegre edilmiştir. Ayrıca oyuncu konforu için diyalogları `Z` tuşuyla geçme ve `ESC` tuşuna 2 saniye basılı tutarak ana menüye dönme gibi global girdi (Input) kontrolleri kodlanmıştır.

---

## 🎨 Görsel Dil ve Atmosfer

Oyunun kasvetli dünyasını ve çöküşünü anlatmak için **Pixel Art** sanat tarzı benimsenmiştir. Renk paleti ağırlıklı olarak gri, bej ve soluk tonlardan seçilerek, çevre tasarımlarındaki yıkılmış yapılar ve cansız arazilerle atmosfer oynanışın bir parçası haline getirilmiştir.

---

## 📁 Kaynak Kodları ve Proje Yapısı

Bu depoda, oyunun çalıştırılabilir halinin yanı sıra projenin asıl kalbini oluşturan temiz kaynak kodları ve asset mimarisi yer almaktadır. `Library` ve `Build` gibi geçici sistem klasörleri `.gitignore` filtreleri ile temizlenerek, sadece incelenebilir temiz proje dosyaları (`Assets`, `Packages`, `ProjectSettings`) paketlenmiştir.
