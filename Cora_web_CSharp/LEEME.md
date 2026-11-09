# Cora · aplicación web en C# (Blazor WebAssembly)

Toda la app está escrita en **C#**: las pantallas (`.razor`), la lógica, las reglas de la IA y el guardado de datos. Se ejecuta en el navegador del ordenador o del móvil.

Incluye los tres retos:

1. **Escanear la etiqueta (QR) con confirmación por sonido y vibración.** El visor se pone verde con ✓ y suena un pitido agudo; si falla, se pone rojo, suenan dos tonos graves y explica cómo arreglarlo. En Android también vibra; en iPhone, Safari no deja vibrar a las páginas web.
2. **Planificador con IA.** Propone un conjunto por día según los planes, el tiempo real de tu ciudad, lo que está en la lavadora y lo que te gusta llevar. Explica por qué y **no repite un conjunto hasta que pasa una semana**. Código: `Servicios/MotorIA.cs`.
3. **Espera percibida.** Mientras la IA trabaja, el calendario muestra huecos grises que brillan, el botón cambia de mensaje y los días se rellenan uno a uno. En Configuración se puede cambiar a la espera «clásica» para comparar. Código: `Servicios/Estado.cs` (`GenerarSemanaAsync`).

---

## A) Probarla en vuestro ordenador

1. Instalad el **SDK de .NET 10**: https://dotnet.microsoft.com/download (botón «.NET 10.0 · SDK»).
2. Abrid una terminal **dentro de esta carpeta** (la que tiene `Cora.Web.csproj`) y escribid:
   ```
   dotnet run
   ```
3. La primera vez tarda un poco, porque descarga lo necesario. Cuando ponga `Now listening on: http://localhost:5245`, abrid esa dirección en el navegador.

Si usáis **Visual Studio 2022/2026**, abrid `Cora.Web.csproj` y pulsad **F5**.

## B) Publicarla en internet con GitHub + Vercel (recomendado)

Así tenéis un enlace https para abrirla en el iPhone; la cámara en directo solo funciona con https. Además, se actualiza sola cada vez que subís un cambio a GitHub.

**1. Subir el código a GitHub**
1. Entrad en https://github.com (cread una cuenta si no tenéis).
2. Arriba a la derecha, **+ › New repository**. Nombre: `cora`. Dejadlo en **Public** y pulsad **Create repository**.
3. En la página del repositorio vacío, pulsad el enlace **«uploading an existing file»**.
4. Arrastrad **todo el contenido de esta carpeta**: los archivos y las carpetas `Layout`, `Modelos`, `Pages`, `Properties`, `Servicios`, `Shared` y `wwwroot`. No subáis las carpetas `bin` ni `obj` si os aparecen.
5. Abajo, pulsad **Commit changes**.

**2. Conectar con Vercel**
1. Entrad en https://vercel.com y pulsad **Sign Up › Continue with GitHub**. El plan gratuito «Hobby» es suficiente.
2. Pulsad **Add New… › Project** y, junto a vuestro repositorio `cora`, **Import**.
3. No hace falta tocar nada: el archivo `vercel.json` ya le dice a Vercel cómo compilar (descarga .NET, compila y publica). Pulsad **Deploy**.
4. La primera vez tarda unos 3–5 minutos. Al terminar sale un enlace del tipo `https://cora-xxxx.vercel.app`.

**3. Abrirla en el iPhone**
- Abrid el enlace en **Safari**.
- Para tenerla como una app: botón **Compartir › Añadir a pantalla de inicio**. Aparecerá el icono de Cora.

**4. Hacer cambios**
Editad un archivo en GitHub (icono del lápiz) o subid la versión nueva, y pulsad **Commit changes**. Vercel la vuelve a publicar sola en un par de minutos.

## Cómo probar cada reto

| Reto | Dónde | Qué hacer |
|---|---|---|
| 1 · QR con sonido | Añadir › Escanear la etiqueta | Escanead una **etiqueta de prueba**: abrid el prototipo en el ordenador › Añadir › Escanear la etiqueta › «Ver etiquetas de prueba», y apuntad con el móvil. Probad también con cualquier otro QR para ver el error. Sin etiquetas, usad «Simular una lectura». |
| 2 · Planificador | Calendario › Planificar con IA | Elegid un día, marcad planes (o escribid «clase y cena con amigas») y pulsad «Generar mi semana». Leed por qué propone cada conjunto. Probad «Otra opción». |
| 3 · La espera | Al generar la semana | Mirad los huecos grises y los mensajes del botón. En Perfil › Configuración se puede cambiar a la espera «Clásica». |

Para oír los sonidos en el iPhone, quitad el modo silencio.

## Qué hay en cada carpeta

```
Pages/        Pantallas: Inicio, Armario, CategoriaPagina, FichaPrenda, Anadir, NuevaFoto,
              NuevaQr (reto 1), NuevaManual, Calendario (reto 3), Planificar (reto 2),
              Explorar, PerfilPagina, Guardados, Configuracion, Registro
Shared/       Piezas reutilizables iguales a Figma: Chip, Cabecera, TarjetaPrenda, FilaOutfit…
Layout/       Marco de la app y barra de navegación inferior
Servicios/    MotorIA (reglas de la IA), Estado (datos, espera, avisos), ServicioTiempo (Open-Meteo), Fechas
Modelos/      Clases (Prenda, Outfit, Perfil…) y datos de ejemplo
wwwroot/      index.html, estilos (css/cora.css), JavaScript (cámara, sonido, vibración) y fotos
vercel.json + vercel-build.sh   Configuración para publicar en Vercel
```

## Lo que hace de verdad y lo que está simulado

- **De verdad:** lectura de QR con la cámara (en directo con https; con foto si no), sonidos, vibración en Android, fotos de prendas, tiempo real de la ciudad del perfil, guardado de todo en el navegador, lavadora (las prendas vuelven a estar disponibles solas a los 2 días) y reglas del planificador.
- **Simulado:** el registro no crea una cuenta en un servidor (cada navegador guarda sus datos); Explorar muestra publicaciones de ejemplo; al hacer una foto la prenda no se reconoce sola; la «IA» es un sistema de reglas, no un modelo de lenguaje.

## Si algo falla

- **Error al compilar:** copiad el mensaje en rojo (con el nombre del archivo y la línea) y mandádmelo.
- **Vercel falla al desplegar:** en Vercel › vuestro proyecto › Deployments › el que falla › **Build Logs**. Copiad las últimas líneas.
- **Empezar de cero con los datos de ejemplo:** Perfil › Configuración › «Restablecer datos de ejemplo».
