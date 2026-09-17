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

Bu kısa adlar GS1 Digital Link standardının 1.2 sürümünde kullanımdan kaldırılmış, 1.3.0 sürümünde standarttan tamamen çıkarılmıştır. Destek, yeni adres üretimini teşvik etmek için değil, eski adreslerle geriye dönük uyumluluğu bilinçli olarak korumak için sürdürülür.

### Karşılaştırma kaynağının niteliği

Fark testinde kullanılan `Solidsoft.Reply.Gs1DigitalLinkLib`, bağımsız bir ikinci standart uygulaması değil, GS1'in JavaScript referansının .NET'e çevrilmiş hâlidir. Bu nedenle iki uygulamanın aynı sonucu vermesi iki bağımsız onay sayılmaz; referans çevirisiyle karşılaştırmada bulunan ayrımlar ise çeviri veya kapsam farklarını görünür kılan tanısal bulgulardır.

Fark testi `Gs1.DigitalLink.DifferentialTests` adlı ayrı projede tutulur ve sabit `20260908` tohumu ile 2.000 geçerli girdi üretir. Her girdide iki URI metnini karşılaştırır, ayrımları test çıktısına kaydeder ve ayrım bulduğunda normal testleri kırmaz. Böylece çözümdeki olağan `dotnet test` ve GitHub Actions akışı yeşil kalır. Tanısal test gerektiğinde ayrıca şu komutla çalıştırılır:

```text
dotnet test Gs1.DigitalLink.DifferentialTests/Gs1.DigitalLink.DifferentialTests.csproj --logger "console;verbosity=detailed"
```

İlk temiz karşılaştırma koşusunda 75 girdinin 37'sinde metinsel fark kaydedildi. Ölçek 2.000 girdiye çıkarıldığında 900 metinsel fark kaydedildi. Gözlenen farklar sorgu parametrelerinin sıralamasıyla sınırlıydı; yol bölümleri, AI/değer eşleşmeleri ve yüzde kodlanmış değerler aynı kaldı. Görev gereği bu farklara dayanarak hiçbir uygulama davranışı değiştirilmedi. Bu ölçüm 15.09.2026 tarihinde `d05ebd2` commit'i üzerinde alındı. Referans test projesinin geçici NuGet önbelleği eksik olduğu ve bu ortamda NuGet kaynağı SSL doğrulamasından geçmediği için yeniden derleme yapılamadı; bu nedenle rapordaki fark sayısı bu çalıştırılabilir sürüme aittir.

## Sapma tablosu

| Fark | Bizim kod | Referans kütüphane | Sonuç |
| --- | --- | --- | --- |
| Sorgu parametrelerinin sırası | Parametreleri AI koduna göre sözlük sırasında diziyor. | Parametrelerin girdi sırasını koruyor. | **Standart belirsiz.** GS1 Digital Link standardının 4.12 maddesi kanonik biçim için sözlük sırasını “should” ile önerir, “shall” ile zorunlu kılmaz. Bu nedenle referansın girdi sırasını koruması geçersiz değildir; iki yaklaşım arasında zorunlu bir doğru/yanlış ayrımı yoktur. |
| AI 11 ve 17 tarih anlamı | Önceden yalnızca `N6` uzunluk ve karakter biçimini denetliyordu; artık ayı, günü ve artık yılı da doğruluyor. | YYMMDD değerini takvim açısından doğruluyor. | **Bizde eksikti, giderildi.** AI 11 ve 17 yalnızca altı rakam değil, tarih anlamı taşıdığı için geçersiz takvim tarihleri reddedilmelidir. |

### AI 11 ve 17 için `DD=00` kararı

Genel amaçlı ve eski verilerle uyumlu kalması gereken bu kütüphane, `DD=00` değerini belirtilen ayın son günü anlamındaki eski GS1 gösterimi olarak kabul eder. Ay yine 01–12 aralığında olmak zorundadır. 1 Ocak 2025 sonrasında düzenlemeye tabi sağlık ürünlerinde gerçek bir gün yazılması gerekliliği uygulama katmanının bağlama özgü doğrulaması olarak bırakılmıştır; genel kütüphanede eski veriyi tüm alanlarda geçersiz kılmak geriye dönük uyumluluğu gereksiz yere bozar.

## Desteklenen kapsam

- JSON kataloğunda tanımlı AI'lar için biçim ve kontrol hanesi doğrulaması
- Parantezli ve ham element string ayrıştırma
- GS ayırıcısıyla değişken uzunluklu ham alanlar
- Sıkıştırılmamış Digital Link URI üretme ve ayrıştırma
- Tam sıkıştırılmış Digital Link URI'ları çözerek doğrulanmış AI/değer listesine dönüştürme
- Birincil anahtar, GTIN nitelendiricisi ve veri özniteliği rolleri
- Kanonik nitelendirici sırası ve deterministik sorgu sırası
- URI değerlerinde yüzde kodlama ve kod çözme
- Özel HTTP/HTTPS taban adresleri ve yol önekleri
- `N`, `N..` ve `X..` katalog biçimleri

## Desteklenmeyen kapsam

- Katalogda bulunmayan GS1 AI'ları
- `gtin` ve `lot` dışındaki kısa yol adları
- Kısmen sıkıştırılmış URI'lar, GS1 dışı anahtar/değer çiftleri ve desteklenen katalog dışındaki sıkıştırma başlıkları
- Digital Link resolver veya ağ üzerinden kaynak çözümleme
- Tüm GS1 ilişki ve kombinasyon kuralları
- Para birimi gibi katalogda ayrı metadata gerektiren daha geniş iş kuralları

Desteklenmeyen girdiler tahmin edilerek dönüştürülmez; doğrulama veya ayrıştırma hatası olarak bildirilir. Bu yaklaşım, geçerli görünmesine rağmen anlamı değişmiş veri üretmekten kaçınmak için seçildi.

## Sıkıştırılmış adres çözme ölçümü

`Solidsoft.Reply.Gs1DigitalLinkLib` ile üretilen tam sıkıştırılmış adresler, sabit `20260915` tohumu kullanılarak 500 geçerli rastgele AI/değer listesiyle denendi. Kütüphanenin mevcut test veri üreticisinin kapsadığı AI'larda 500 girdinin tamamı doğru çözüldü; çözülemeyen girdi ve kaydedilecek hata nedeni oluşmadı. Bu sonuç yalnızca katalogdaki mevcut AI'ları ve uygulanan tam sıkıştırma başlıklarını kapsar; kısmi sıkıştırma ve GS1 dışı çiftler destek kapsamı dışındadır. Ölçüm 15.09.2026 tarihinde `d05ebd2` commit'i üzerinde alındı.

## Sıkıştırma uzunluk ölçümü

Sabit `20260915` tohumu ile üretilen aynı 500 girdi için tam ve sıkıştırılmış URI uzunlukları karşılaştırıldı. Ortalama kısalma **%24,86** oldu. En az kısalan örnek **%13,43** ile 67 karakterden 58 karaktere düştü: `(01)17150572614010(10)HB(22)5xh6n&44dEV7HQVss2w`. En çok kısalan örnek **%36,25** ile 80 karakterden 51 karaktere düştü: `(17)210821(01)90567149172409(3103)804549(21)&=pX(11)280606`. Ölçüm 17.09.2026 tarihinde, işlevsel kodun bulunduğu `d05ebd2` commit'i derlemesinden alınmıştır.

## Bilinen sınırlar

1. AI 10 değeri `LOT/1` olan bir tanım oluşturulduğunda eğik çizgi yol ayırıcısı olarak değil `%2F` olarak kodlanır. Kaydedilen `/01/08690504080008/10/LOT%2F1` yolu aynı biçimde istekle geldiğinde kayıt bulundu ve `307` yönlendirmesi döndü. Bu denemede yol eşleşmesi sorunu görülmedi.
2. Test verisi üreticisinin karakter kümesi `CSET 82` içindeki çift tırnak, tek tırnak, açma parantezi, kapama parantezi ve yıldızı üretmez. Bu nedenle 2.000 girdilik fark testi ve 500 girdilik sıkıştırma çözme testi bu beş karakter için kanıt oluşturmaz; `500/500` sonucu bu sınırla birlikte değerlendirilmelidir.
3. Hedef dili `tr-TR`, istek başlığı `Accept-Language: tr` olduğunda mevcut eşleştirme iki yönlü değildir: `tr`, `tr-TR` ile eşleşmez ve seçim varsayılan hedefe düşer. Ters yönde, daha ayrıntılı istek dili daha genel hedef dili kapsayabilir.
4. Açıkça istenen bağlantı tipi için dil veya medya tipi eşleşmezse seçim, tanımın varsayılan hedefine düşer. Varsayılan hedef farklı bir bağlantı tipine sahip olabilir; bu bilinçli geri dönüş davranışıdır.
5. Tam sıkıştırılmış Digital Link çözümleyicisindeki başlık tablosu, standardın bütün başlık ve AI birleşimlerini değil, mevcut katalog ve test verisinin kullandığı kümeyi kapsar.
6. Sıkıştırılmış Digital Link desteği yalnızca çözme yönündedir; kütüphane sıkıştırılmış URI üretmez.
