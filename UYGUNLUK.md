# GS1 uyumluluk durumu

Bu dosya kütüphanenin mevcut kapsamını, GS1'in JavaScript referans deposundaki örneklerle yapılan karşılaştırmanın sonucunu ve bilinen sınırları kaydeder. Tam GS1 standardı uyumluluğu iddia edilmez.

## Resmî örneklerle doğrulananlar

`OfficialGs1CompatibilityTests` içinde üç uçtan uca örnek kalıcı test olarak tutulur:

- Kanonik olmayan sıradaki ham element string ayrıştırılır ve birincil anahtar, nitelendirici ve veri öznitelikleri doğru URI bölümlerine yerleştirilir.
- Digital Link URI, yüzde kodlaması çözülerek sıralı AI/değer listesine çevrilir. `%26` tekrar `&`, `%2B` tekrar `+` olur.
- `gtin` ve `lot` kısa adlarını içeren, özel yol önekli URI ayrıştırılır; 12 haneli GTIN başına iki sıfır eklenerek 14 haneye normalleştirilir.

İlk test çalıştırmasında üç resmî örnek de kırmızıydı. Kayıt altına alınan nedenler şunlardı:

- 3923 katalogda bulunmuyordu.
- Biçim ayrıştırıcı değişken uzunluklu sayısal `N..15` biçimini tanımıyordu.
- URI ayrıştırıcı `gtin` ve `lot` kısa adlarını tanımıyordu.

3923, `N..15` biçimli değişken uzunluklu sayısal bir veri özniteliği olarak kataloğa eklendi. Boş değer, 15 haneden uzun değer ve rakam dışı karakterler reddedilir.

## Tasarım kararları

### 12 haneli GTIN

Digital Link adres ayrıştırıcısı, AI 01 için 12 haneli geçerli bir GTIN aldığında başına iki sıfır ekleyerek 14 haneye normalleştirir. Kütüphanenin iç temsilini tek biçimde tutmak ve resmî örnekteki URI ile element string arasında kayıpsız dönüşüm sağlamak için bu destek eklendi. Üretici ve element string doğrulayıcısı iç temsilde yine 14 hane bekler.

### Kısa adlar

Yalnızca resmî örnekte kullanılan küçük harfli `gtin` ve `lot` yol adları desteklenir ve sırasıyla 01 ile 10'a çevrilir. Katalogdaki bütün AI'lar için tahmine dayalı adlar eklenmedi; belgelenmemiş adların sessizce yanlış AI'a eşlenmesini önlemek için diğer adlar reddedilir.

## Desteklenen kapsam

- JSON kataloğunda tanımlı AI'lar için biçim ve kontrol hanesi doğrulaması
- Parantezli ve ham element string ayrıştırma
- GS ayırıcısıyla değişken uzunluklu ham alanlar
- Sıkıştırılmamış Digital Link URI üretme ve ayrıştırma
- Birincil anahtar, GTIN nitelendiricisi ve veri özniteliği rolleri
- Kanonik nitelendirici sırası ve deterministik sorgu sırası
- URI değerlerinde yüzde kodlama ve kod çözme
- Özel HTTP/HTTPS taban adresleri ve yol önekleri
- `N`, `N..` ve `X..` katalog biçimleri

## Desteklenmeyen kapsam

- Katalogda bulunmayan GS1 AI'ları
- `gtin` ve `lot` dışındaki kısa yol adları
- Sıkıştırılmış Digital Link URI biçimleri
- Digital Link resolver veya ağ üzerinden kaynak çözümleme
- Tüm GS1 ilişki ve kombinasyon kuralları
- Para birimi gibi katalogda ayrı metadata gerektiren daha geniş iş kuralları

Desteklenmeyen girdiler tahmin edilerek dönüştürülmez; doğrulama veya ayrıştırma hatası olarak bildirilir. Bu yaklaşım, geçerli görünmesine rağmen anlamı değişmiş veri üretmekten kaçınmak için seçildi.
