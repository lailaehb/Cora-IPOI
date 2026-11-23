// Idiomas de Cora: español (el de la app), inglés y francés.
// Cómo funciona: la app se escribe en español y, si la persona elige otro idioma en Configuración,
// este archivo traduce los textos al aparecer en pantalla (y los vuelve a traducir cuando cambian).
// Así ninguna pantalla se queda sin traducir y no hace falta tocar cada archivo de la app.
// Lo que escriben las personas en Explorar (sus textos, bios y comentarios) se deja en su idioma, como en una red social.
window.coraIdiomas = (function () {
  // ---------------------------------------------------------------- textos exactos: español → [inglés, francés]
  const T = {
    // Menú de abajo y secciones
    'Inicio': ['Home', 'Accueil'], 'Armario': ['Wardrobe', 'Garde-robe'], 'Mi ropa': ['My clothes', 'Mes vêtements'],
    'Añadir': ['Add', 'Ajouter'], 'Añadir ropa': ['Add clothes', 'Ajouter'], 'Calendario': ['Calendar', 'Calendrier'],
    'Mis días': ['My days', 'Mes jours'], 'Explorar': ['Explore', 'Explorer'], 'Mi perfil': ['My profile', 'Mon profil'],
    'Secciones': ['Sections', 'Sections'], 'Añadir prenda': ['Add item', 'Ajouter un vêtement'], 'Volver': ['Back', 'Retour'],
    'Perfil': ['Profile', 'Profil'], 'Configuración': ['Settings', 'Paramètres'],

    // Registro
    'tu armario, a tu manera': ['your wardrobe, your way', 'ta garde-robe, à ta façon'],
    'Crea tu cuenta': ['Create your account', 'Crée ton compte'], 'Nombre': ['Name', 'Nom'],
    'Correo electrónico': ['Email', 'E-mail'], 'Contraseña': ['Password', 'Mot de passe'], 'Continuar': ['Continue', 'Continuer'],
    'Continuar con Google': ['Continue with Google', 'Continuer avec Google'], '¿Ya tienes cuenta? Entrar': ['Already have an account? Log in', 'Déjà un compte ? Se connecter'],
    'Entrar': ['Log in', 'Se connecter'], 'Cuéntanos sobre ti': ['Tell us about yourself', 'Parle-nous de toi'],
    'Edad': ['Age', 'Âge'], 'Ciudad': ['City', 'Ville'], 'Ciudad (para el tiempo)': ['City (for the weather)', 'Ville (pour la météo)'],
    '¿Cuál es tu estilo?': ["What's your style?", 'Quel est ton style ?'], '¿Qué te gusta llevar?': ['What do you like to wear?', "Qu'aimes-tu porter ?"],
    'Así Cora sabe qué tiempo hace y qué ropa te pega. Podrás cambiarlo en tu perfil.': ['This way Cora knows the weather and what suits you. You can change it in your profile.', 'Ainsi Cora connaît la météo et ce qui te va. Tu pourras le changer dans ton profil.'],
    'Empezar': ['Get started', 'Commencer'], 'Escribe tu nombre.': ['Write your name.', 'Écris ton nom.'],
    'Escribe un correo válido, por ejemplo pilar@correo.es.': ['Enter a valid email, for example pilar@mail.com.', 'Écris un e-mail valide, par exemple pilar@mail.fr.'],
    'La contraseña necesita al menos 6 caracteres.': ['The password needs at least 6 characters.', 'Le mot de passe doit contenir au moins 6 caractères.'],
    'Tu nombre': ['Your name', 'Ton nom'], 'Tu foto': ['Your photo', 'Ta photo'],

    // Inicio
    'Buenos días': ['Good morning', 'Bonjour'], 'Buenas tardes': ['Good afternoon', 'Bon après-midi'], 'Buenas noches': ['Good evening', 'Bonsoir'],
    'Tu outfit de hoy': ["Today's outfit", 'Ta tenue du jour'], 'Tu ropa de hoy': ["Today's clothes", 'Tes vêtements du jour'],
    'Otra opción': ['Another option', 'Autre option'], 'Ver otra': ['See another', 'Voir une autre'], 'Guardar': ['Save', 'Enregistrer'],
    'Guardado': ['Saved', 'Enregistré'], 'Guardado ✓': ['Saved ✓', 'Enregistré ✓'], 'Pensando…': ['Thinking…', 'Je réfléchis…'],
    'Esta semana': ['This week', 'Cette semaine'], 'Próximos días': ['Next days', 'Prochains jours'], 'Ver calendario': ['See calendar', 'Voir le calendrier'],
    'Planificar': ['Plan', 'Planifier'], 'Elegir': ['Choose', 'Choisir'], 'Cambio de ropa': ['Change of clothes', 'Changement de tenue'],
    'Elegido por la IA con tu armario': ['Chosen by AI from your wardrobe', "Choisi par l'IA avec ta garde-robe"],
    'Hoy toca poner la lavadora': ["Today is laundry day", "Aujourd'hui, c'est lessive"],
    'Toca para poner la lavadora': ['Tap to start the laundry', 'Touche pour lancer la lessive'],
    'Toca para marcarlas como disponibles': ['Tap to mark them as available', 'Touche pour les marquer comme disponibles'],

    // Armario y categorías
    'Mi armario': ['My wardrobe', 'Ma garde-robe'], 'Categorías': ['Categories', 'Catégories'], 'Todas': ['All', 'Toutes'],
    'Añadidas hace poco': ['Recently added', 'Ajoutés récemment'], 'Arriba': ['Tops', 'Hauts'], 'Abajo': ['Bottoms', 'Bas'],
    'Calzado': ['Shoes', 'Chaussures'], 'Abrigos': ['Coats', 'Manteaux'], 'Abrigos y chaquetas': ['Coats and jackets', 'Manteaux et vestes'],
    'Parte de arriba': ['Tops', 'Hauts'], 'Parte de abajo': ['Bottoms', 'Bas'],
    'Camisetas': ['T-shirts', 'T-shirts'], 'Tops': ['Tops', 'Tops'], 'Camisas': ['Shirts', 'Chemises'], 'Sudaderas': ['Sweatshirts', 'Sweats'],
    'Jerséis': ['Sweaters', 'Pulls'], 'Deportiva': ['Sportswear', 'Sport'], 'Vaqueros': ['Jeans', 'Jeans'], 'Pantalones': ['Trousers', 'Pantalons'],
    'Faldas': ['Skirts', 'Jupes'], 'Mallas': ['Leggings', 'Leggings'], 'Shorts': ['Shorts', 'Shorts'], 'Zapatillas': ['Sneakers', 'Baskets'],
    'Botas': ['Boots', 'Bottes'], 'Tacones': ['Heels', 'Talons'], 'Chaquetas': ['Jackets', 'Vestes'], 'Blazers': ['Blazers', 'Blazers'],
    'Cesto de la ropa': ['Laundry basket', 'Panier à linge'], 'Ropa sucia': ['Dirty clothes', 'Linge sale'], 'En el cesto': ['In the basket', 'Dans le panier'],
    'En la lavadora': ['In the washer', 'Dans la machine'], 'Ir al cesto': ['Go to basket', 'Aller au panier'],
    'Seleccionar varias': ['Select several', 'En sélectionner plusieurs'], 'Terminar': ['Done', 'Terminer'], 'Cancelar': ['Cancel', 'Annuler'],
    'Lavar': ['Wash', 'Laver'], 'Liberar': ['Free up', 'Libérer'], 'Disponible': ['Available', 'Disponible'],
    'Cora no las usará hasta que estén listas': ["Cora won't use them until they're ready", 'Cora ne les utilisera pas avant qu’ils soient prêts'],
    'No tienes nada en la lavadora. Todo tu armario está disponible.': ['Nothing in the washer. Your whole wardrobe is available.', "Rien dans la machine. Toute ta garde-robe est disponible."],
    'Marcar todas como disponibles': ['Mark all as available', 'Tout marquer comme disponible'],

    // Ficha de la prenda y formulario
    'Ficha de la prenda': ['Item details', 'Fiche du vêtement'], 'Favorita': ['Favourite', 'Favori'], 'Favoritas': ['Favourites', 'Favoris'],
    'Añadida a favoritas': ['Added to favourites', 'Ajouté aux favoris'], 'Echar al cesto': ['Put in the basket', 'Mettre au panier'],
    'Está sucia': ["It's dirty", 'Il est sale'], 'Sacar del cesto': ['Take out of basket', 'Sortir du panier'],
    'Marcar como disponible': ['Mark as available', 'Marquer comme disponible'], 'Ya está limpia': ["It's clean now", 'Il est propre'],
    'Ya está seca': ["It's dry", 'Il est sec'], 'Ya seca': ['Already dry', 'Déjà sec'], 'Lavado y secado': ['Washed and dried', 'Lavé et séché'],
    'Ya debería estar seca': ['It should be dry by now', 'Il devrait être sec'], 'Disponible de nuevo': ['Available again', 'De nouveau disponible'],
    'Borrar prenda': ['Delete item', 'Supprimer le vêtement'], 'Borrar': ['Delete', 'Supprimer'], 'Quitar': ['Remove', 'Retirer'],
    'Esta prenda ya no está en tu armario.': ['This item is no longer in your wardrobe.', "Ce vêtement n'est plus dans ta garde-robe."],
    'Cora la puede usar en tus conjuntos.': ['Cora can use it in your outfits.', 'Cora peut l’utiliser dans tes tenues.'],
    'Está sucia, así que Cora no la usará en tus conjuntos. Irá a la lavadora con el resto del cesto.': ["It's dirty, so Cora won't use it in your outfits. It will go in the washer with the rest of the basket.", "Il est sale, donc Cora ne l'utilisera pas. Il ira dans la machine avec le reste du panier."],
    'Al cesto. Cuando pongas la lavadora, irá con el resto.': ['In the basket. When you do the laundry, it goes with the rest.', 'Au panier. Quand tu lanceras la machine, il ira avec le reste.'],
    'Ya ha pasado el tiempo que elegiste al poner la lavadora.': ['The time you chose when starting the laundry has passed.', 'Le temps choisi pour la lessive est écoulé.'],
    'Nombre de la prenda *': ['Item name *', 'Nom du vêtement *'], 'Marca': ['Brand', 'Marque'], 'Talla': ['Size', 'Taille'], 'Color': ['Colour', 'Couleur'],
    'Categoría': ['Category', 'Catégorie'], 'Categoría *': ['Category *', 'Catégorie *'], 'Tipo': ['Type', 'Type'],
    '¿Para qué la usas?': ['What do you use it for?', "Pour quoi l'utilises-tu ?"],
    'Se marcan seguidos, de lo más cómodo a lo más arreglado.': ['They are marked in a row, from most casual to most dressy.', 'Ils se suivent, du plus confortable au plus habillé.'],
    'Cora solo te la propondrá para estos planes.': ['Cora will only suggest it for these plans.', 'Cora ne la proposera que pour ces projets.'],
    'Para quitar ese uso, quita antes los de los extremos.': ['To remove that use, first remove the ones at the ends.', "Pour retirer cet usage, retire d'abord ceux des extrémités."],
    'Por ejemplo: camiseta de rayas': ['For example: striped T-shirt', 'Par exemple : t-shirt rayé'],
    'Escribe un nombre para la prenda.': ['Write a name for the item.', 'Écris un nom pour le vêtement.'], 'Elige una categoría.': ['Choose a category.', 'Choisis une catégorie.'],
    'Elige para qué usas la prenda.': ['Choose what you use the item for.', "Choisis à quoi sert le vêtement."],
    'La prenda tiene que servir para algún plan.': ['The item has to work for at least one plan.', 'Le vêtement doit servir pour au moins un projet.'],
    'Deporte': ['Sport', 'Sport'], 'Diario': ['Everyday', 'Quotidien'], 'Arreglado': ['Dressy', 'Habillé'], 'Fiesta': ['Party', 'Fête'],
    'Guardar en mi armario': ['Save to my wardrobe', 'Enregistrer dans ma garde-robe'], 'Revisa la prenda': ['Check the item', 'Vérifie le vêtement'],
    'Completa los datos para que Cora la tenga en cuenta': ['Complete the details so Cora can use it', 'Complète les infos pour que Cora en tienne compte'],
    'Añadir una foto (opcional)': ['Add a photo (optional)', 'Ajouter une photo (facultatif)'], 'Cambiar la foto': ['Change photo', 'Changer la photo'],

    // Añadir
    '¿Cómo quieres añadirla?': ['How do you want to add it?', 'Comment veux-tu l’ajouter ?'], 'Escanear la etiqueta': ['Scan the label', "Scanner l'étiquette"],
    'Lee el código QR y rellena los datos por ti': ['Reads the QR code and fills in the details for you', 'Lit le QR code et remplit les infos pour toi'],
    'Hacer una foto': ['Take a photo', 'Prendre une photo'], 'Se abre la cámara y luego completas los datos': ['Opens the camera, then you complete the details', "Ouvre l'appareil photo, puis tu complètes les infos"],
    'Añadir a mano': ['Add manually', 'Ajouter à la main'], 'Escribe tú los datos con el teclado': ['Type the details yourself', 'Saisis toi-même les infos'],
    'Así tu armario siempre estará completo': ['So your wardrobe is always complete', 'Ainsi ta garde-robe sera toujours complète'],
    'Escanear etiqueta': ['Scan label', 'Scanner l’étiquette'], 'Coloca el código QR dentro del cuadro': ['Place the QR code inside the frame', 'Place le QR code dans le cadre'],
    'Haz una foto al código QR de la etiqueta': ['Take a photo of the QR code on the label', "Prends en photo le QR code de l'étiquette"],
    'Etiqueta leída': ['Label read', 'Étiquette lue'], 'No se ha podido leer': ["Couldn't read it", 'Lecture impossible'],
    'Hacer una foto a la etiqueta': ['Take a photo of the label', "Prendre l'étiquette en photo"], 'Escanear otra': ['Scan another', 'En scanner une autre'],
    'Volver a intentarlo': ['Try again', 'Réessayer'], 'Simular una lectura (sin etiqueta)': ['Simulate a scan (no label)', 'Simuler une lecture (sans étiquette)'],
    'Hacer una foto a la prenda (opcional)': ['Take a photo of the item (optional)', 'Prendre le vêtement en photo (facultatif)'],
    'Revisa los datos antes de guardar. Puedes corregir lo que haga falta.': ['Check the details before saving. You can correct anything.', "Vérifie les infos avant d'enregistrer. Tu peux corriger ce qu'il faut."],
    'Este código no es de una prenda.': ["This code isn't for a clothing item.", "Ce code ne correspond pas à un vêtement."],
    'No encontramos un código QR en la foto.': ["We couldn't find a QR code in the photo.", "Aucun QR code trouvé sur la photo."],
    'Acerca la etiqueta para que el código ocupe casi toda la foto, con buena luz y sin reflejos.': ['Bring the label closer so the code fills most of the photo, with good light and no glare.', "Approche l'étiquette pour que le code remplisse presque toute la photo, avec une bonne lumière et sans reflets."],
    'No hace falta añadirla otra vez. Puedes escanear otra prenda.': ["No need to add it again. You can scan another item.", "Inutile de l'ajouter à nouveau. Tu peux scanner un autre vêtement."],
    'Cora entiende los QR de las etiquetas de Cora y los enlaces de producto de cualquier tienda online. Prueba con otro.': ['Cora understands Cora label QR codes and product links from any online shop. Try another one.', "Cora comprend les QR des étiquettes Cora et les liens produits de n'importe quelle boutique en ligne. Essaie un autre."],
    'No hay permiso para usar la cámara. Actívalo en los ajustes del navegador, o usa «Hacer una foto a la etiqueta».': ['No permission to use the camera. Turn it on in the browser settings, or use “Take a photo of the label”.', "Pas d'autorisation pour la caméra. Active-la dans les réglages du navigateur, ou utilise « Prendre l'étiquette en photo »."],
    'Hacer la foto': ['Take the photo', 'Prendre la photo'],
    'Pon la prenda sobre un fondo liso, con buena luz, y que ocupe casi toda la foto': ['Place the item on a plain background, with good light, filling most of the photo', 'Pose le vêtement sur un fond uni, bien éclairé, et qu’il remplisse presque toute la photo'],
    'He rellenado los datos a partir de la foto. Revísalos antes de guardar.': ["I've filled in the details from the photo. Check them before saving.", "J'ai rempli les infos à partir de la photo. Vérifie-les avant d'enregistrer."],
    'No veo ninguna prenda en la foto. Puedes repetirla o rellenar los datos a mano.': ["I can't see any clothing in the photo. You can retake it or fill in the details manually.", "Je ne vois aucun vêtement sur la photo. Tu peux la refaire ou remplir les infos à la main."],
    'No se ha podido abrir la foto. Prueba con otra.': ["Couldn't open the photo. Try another one.", "Impossible d'ouvrir la photo. Essaie une autre."],

    // Esperas (reto 3)
    'Cargando…': ['Loading…', 'Chargement…'], 'Cargando': ['Loading', 'Chargement'],
    'Mirando el tiempo de la semana…': ["Checking the week's weather…", 'Je regarde la météo de la semaine…'],
    'Descartando lo que está en la lavadora…': ["Leaving out what's in the washer…", 'J’écarte ce qui est dans la machine…'],
    'Pidiendo ideas a la IA…': ['Asking the AI for ideas…', "Je demande des idées à l'IA…"], 'Combinando tus prendas…': ['Combining your clothes…', 'Je combine tes vêtements…'],
    'Buscando otra combinación…': ['Looking for another combination…', 'Je cherche une autre combinaison…'],
    'Probando con otras prendas…': ['Trying other items…', "J'essaie d'autres vêtements…"], 'Teniendo en cuenta el tiempo…': ['Taking the weather into account…', 'Je tiens compte de la météo…'],
    'Casi lo tengo…': ['Almost there…', "J'y suis presque…"], 'Cora está mirando tu foto…': ['Cora is looking at your photo…', 'Cora regarde ta photo…'],
    'Reconociendo la prenda…': ['Recognising the item…', 'Je reconnais le vêtement…'], 'Fijándome en el color…': ['Looking at the colour…', 'Je regarde la couleur…'],
    'Eligiendo la categoría…': ['Choosing the category…', 'Je choisis la catégorie…'],
    'Tu outfit está listo': ['Your outfit is ready', 'Ta tenue est prête'], 'Tu semana está lista': ['Your week is ready', 'Ta semaine est prête'],

    // Calendario y día
    'Tus outfits del mes': ["This month's outfits", 'Tes tenues du mois'], 'Mes': ['Month', 'Mois'], 'Mes anterior': ['Previous month', 'Mois précédent'],
    'Mes siguiente': ['Next month', 'Mois suivant'], 'Con outfit': ['With outfit', 'Avec tenue'], 'Con planes': ['With plans', 'Avec projets'],
    'Toca un día para ver tu ropa y contarle tus planes a la IA.': ['Tap a day to see your clothes and tell the AI your plans.', "Touche un jour pour voir tes vêtements et raconter tes projets à l'IA."],
    'Toca un día para elegir tu ropa.': ['Tap a day to choose your clothes.', 'Touche un jour pour choisir tes vêtements.'],
    'Mi semana': ['My week', 'Ma semaine'], 'Sin planes': ['No plans', 'Pas de projets'], 'Sin outfit registrado.': ['No outfit recorded.', 'Aucune tenue enregistrée.'],
    'Todavía sin outfit.': ['No outfit yet.', 'Pas encore de tenue.'], 'Contarle mis planes a la IA': ['Tell the AI my plans', "Raconter mes projets à l'IA"],
    'Elegir mi ropa': ['Choose my clothes', 'Choisir mes vêtements'], 'Tu ropa': ['Your clothes', 'Tes vêtements'],
    'Este día no tiene outfit registrado.': ["This day has no outfit recorded.", "Ce jour n'a pas de tenue enregistrée."],
    'Todavía sin outfit. Cuéntale tus planes a Cora y te lo prepara.': ['No outfit yet. Tell Cora your plans and she will prepare one.', 'Pas encore de tenue. Raconte tes projets à Cora et elle la prépare.'],
    '¿Qué vas a hacer?': ['What are you going to do?', 'Que vas-tu faire ?'], 'O cuéntaselo con tus palabras': ['Or tell it in your own words', 'Ou raconte-le avec tes mots'],
    'Por ejemplo: clase por la mañana y gimnasio por la tarde': ['For example: class in the morning and gym in the afternoon', 'Par exemple : cours le matin et salle de sport l’après-midi'],
    '¿Qué quieres que prepare la IA?': ['What do you want the AI to prepare?', "Que veux-tu que l'IA prépare ?"], 'Qué planificar': ['What to plan', 'Quoi planifier'],
    'Solo este día': ['Just this day', 'Ce jour seulement'], 'Toda la semana': ['The whole week', 'Toute la semaine'], 'Días de la semana': ['Days of the week', 'Jours de la semaine'],
    'Ningún día de esta semana tiene planes todavía. Toca cada día arriba y cuéntale qué vas a hacer.': ["No day this week has plans yet. Tap each day above and tell it what you're doing.", "Aucun jour de cette semaine n'a encore de projets. Touche chaque jour ci-dessus et raconte ce que tu vas faire."],
    'Cuéntale a Cora el plan de al menos un día.': ['Tell Cora the plan for at least one day.', 'Raconte à Cora le projet d’au moins un jour.'],
    'Preparar la semana (1 día con planes)': ['Prepare the week (1 day with plans)', 'Préparer la semaine (1 jour avec projets)'],
    'Universidad': ['University', 'Université'], 'Trabajo': ['Work', 'Travail'], 'Cena': ['Dinner', 'Dîner'],

    // Explorar
    'Buscar personas u outfits': ['Search people or outfits', 'Rechercher des personnes ou des tenues'], 'Borrar búsqueda': ['Clear search', 'Effacer la recherche'],
    'Comprobando…': ['Checking…', 'Vérification…'], 'Escribe una edad real, entre 13 y 100 años.': ['Enter a real age, between 13 and 100.', 'Indique un âge réel, entre 13 et 100 ans.'],
    'Escribe tu ciudad para que Cora sepa qué tiempo hace.': ['Enter your city so Cora knows the weather.', 'Indique ta ville pour que Cora connaisse la météo.'],
    'La ciudad solo puede tener letras, por ejemplo «Ciudad Real».': ['The city can only contain letters, for example “Paris”.', 'La ville ne peut contenir que des lettres, par exemple « Paris ».'],
    'El nombre solo puede tener letras.': ['The name can only contain letters.', 'Le nom ne peut contenir que des lettres.'],
    'Inspírate con los conjuntos de otras personas': ["Get inspired by other people's outfits", "Inspire-toi des tenues des autres"],
    'Inspírate': ['Get inspired', 'Inspire-toi'], 'Qué ver': ['What to see', 'Quoi voir'], 'Todo': ['All', 'Tout'], 'Outfits': ['Outfits', 'Tenues'],
    'Personas': ['People', 'Personnes'], 'Personas que quizá te gusten': ['People you may like', 'Des personnes qui pourraient te plaire'],
    'Ver todas': ['See all', 'Voir tout'], 'Últimos outfits': ['Latest outfits', 'Dernières tenues'], 'Seguir': ['Follow', 'Suivre'],
    '✓ Siguiendo': ['✓ Following', '✓ Abonné(e)'], 'Sus outfits': ['Their outfits', 'Ses tenues'], 'outfits': ['outfits', 'tenues'], 'outfit': ['outfit', 'tenue'],
    'seguidores': ['followers', 'abonnés'], 'siguiendo': ['following', 'abonnements'], 'Me gusta': ['Like', "J'aime"],
    'Guardar en mi perfil': ['Save to my profile', 'Enregistrer dans mon profil'], 'Foto del conjunto': ['Outfit photo', 'Photo de la tenue'],
    'Outfit': ['Outfit', 'Tenue'], 'Publicar': ['Post', 'Publier'], 'Escribe un comentario…': ['Write a comment…', 'Écris un commentaire…'],
    'Escribe un comentario': ['Write a comment', 'Écris un commentaire'], 'Tú': ['You', 'Toi'], 'Borrar mi comentario': ['Delete my comment', 'Supprimer mon commentaire'],
    'Comentario publicado': ['Comment posted', 'Commentaire publié'], 'Todavía no hay comentarios. ¡Escribe el primero!': ['No comments yet. Be the first!', 'Pas encore de commentaires. Écris le premier !'],
    'No encontramos a esta persona.': ["We couldn't find this person.", 'Personne introuvable.'], 'Esta publicación ya no existe.': ['This post no longer exists.', "Cette publication n'existe plus."],
    'ahora': ['now', "à l'instant"], 'ayer': ['yesterday', 'hier'], 'hace 1 semana': ['1 week ago', 'il y a 1 semaine'],

    // Perfil y guardados
    'Editar perfil': ['Edit profile', 'Modifier le profil'], 'Mis outfits': ['My outfits', 'Mes tenues'], 'Mis prendas': ['My clothes', 'Mes vêtements'],
    'Prendas favoritas': ['Favourite items', 'Vêtements favoris'], 'Opciones del perfil': ['Profile options', 'Options du profil'],
    'Modo fácil, colores, avisos y lavadora': ['Easy mode, colours, alerts and laundry', 'Mode facile, couleurs, alertes et lessive'],
    'Aún no has elegido tu estilo.': ["You haven't chosen your style yet.", "Tu n'as pas encore choisi ton style."],
    'Poner foto de perfil': ['Add profile photo', 'Ajouter une photo de profil'], 'Cambiar foto de perfil': ['Change profile photo', 'Changer la photo de profil'],
    'Foto de perfil cambiada': ['Profile photo changed', 'Photo de profil modifiée'], 'Guardados': ['Saved', 'Enregistrés'],
    'Aún no has guardado ningún outfit. Pulsa «Guardar» en un conjunto de la IA o en una publicación de Explorar.': ["You haven't saved any outfits yet. Tap “Save” on an AI outfit or an Explore post.", "Tu n'as encore enregistré aucune tenue. Touche « Enregistrer » sur une tenue de l'IA ou une publication d'Explorer."],
    'Marca una prenda con el corazón en su ficha para tenerla aquí.': ['Mark an item with the heart on its page to have it here.', 'Marque un vêtement avec le cœur sur sa fiche pour l’avoir ici.'],
    'Guardado en tu perfil': ['Saved to your profile', 'Enregistré dans ton profil'], 'Guardado en tu perfil, en Outfits guardados': ['Saved to your profile, in Saved outfits', 'Enregistré dans ton profil, dans Tenues enregistrées'],
    'Pulsa «Guardar» en un outfit de Explorar para tenerlo aquí.': ['Tap “Save” on an Explore outfit to have it here.', "Touche « Enregistrer » sur une tenue d'Explorer pour l'avoir ici."],
    'Aún no tienes outfits. En el calendario, entra en un día y pulsa «Guardar» en el conjunto que te guste.': ["You don't have any outfits yet. In the calendar, open a day and tap “Save” on the outfit you like.", "Tu n'as pas encore de tenues. Dans le calendrier, ouvre un jour et touche « Enregistrer » sur la tenue qui te plaît."],
    'De Explorar': ['From Explore', "D'Explorer"],

    // Configuración
    'Modo fácil y accesibilidad': ['Easy mode and accessibility', 'Mode facile et accessibilité'], 'Modo fácil': ['Easy mode', 'Mode facile'],
    'Letra y botones más grandes, palabras más sencillas y menos cosas en pantalla.': ['Bigger text and buttons, simpler words and fewer things on screen.', "Texte et boutons plus grands, mots plus simples et moins d'éléments à l'écran."],
    'Ver': ['See', 'Voir'], 'Tamaño de la letra': ['Text size', 'Taille du texte'], 'Normal': ['Normal', 'Normal'], 'Grande': ['Large', 'Grand'], 'Muy grande': ['Very large', 'Très grand'],
    'Colores': ['Colours', 'Couleurs'], 'Claro': ['Light', 'Clair'], 'Oscuro': ['Dark', 'Sombre'], 'Como el móvil': ['Like my phone', 'Comme le téléphone'],
    'Alto contraste': ['High contrast', 'Contraste élevé'], 'Textos y bordes más marcados.': ['Stronger text and borders.', 'Textes et bordures plus marqués.'],
    'Tocar': ['Touch', 'Toucher'], 'Botones más grandes': ['Bigger buttons', 'Boutons plus grands'], 'Más fáciles de pulsar y más separados.': ['Easier to tap and further apart.', 'Plus faciles à toucher et plus espacés.'],
    'Mensajes más largos': ['Longer messages', 'Messages plus longs'], 'Los avisos de abajo se quedan más tiempo en pantalla.': ['Notices at the bottom stay on screen longer.', "Les avis en bas restent plus longtemps à l'écran."],
    'Entender': ['Understand', 'Comprendre'], 'Palabras más sencillas': ['Simpler words', 'Mots plus simples'],
    'Menos frases y palabras fáciles, por ejemplo «Ropa sucia» en vez de «Cesto».': ['Fewer sentences and easy words, for example “Dirty clothes” instead of “Basket”.', 'Moins de phrases et des mots faciles, par exemple « Linge sale » au lieu de « Panier ».'],
    'Menos cosas en pantalla': ['Fewer things on screen', "Moins d'éléments à l'écran"], 'Oculta Explorar y otras opciones que no son básicas.': ['Hides Explore and other non-essential options.', 'Masque Explorer et les autres options non essentielles.'],
    'Los cambios se ven al momento. Puedes volver a dejarlo como estaba cuando quieras.': ['Changes apply instantly. You can undo them whenever you like.', "Les changements s'appliquent tout de suite. Tu peux revenir en arrière quand tu veux."],
    'Modo fácil activado': ['Easy mode on', 'Mode facile activé'], 'Modo fácil desactivado': ['Easy mode off', 'Mode facile désactivé'],
    'Idioma': ['Language', 'Langue'], 'Tu cuenta': ['Your account', 'Ton compte'], 'Usuario': ['Username', "Nom d'utilisateur"], 'Cambiar': ['Change', 'Modifier'],
    'Cuenta vinculada': ['Linked account', 'Compte associé'], 'Correo': ['Email', 'E-mail'], 'Sobre ti y tu estilo': ['About you and your style', 'Toi et ton style'],
    'Sobre ti (sale en tu perfil)': ['About you (shown on your profile)', 'À propos de toi (visible sur ton profil)'],
    'Por ejemplo: estudiante, me encantan los vaqueros': ['For example: student, I love jeans', "Par exemple : étudiante, j'adore les jeans"],
    'Tu estilo': ['Your style', 'Ton style'], 'Lo que más te gusta llevar': ['What you like wearing most', "Ce que tu aimes le plus porter"],
    'Guardar mis datos': ['Save my details', 'Enregistrer mes infos'], 'Datos guardados': ['Details saved', 'Infos enregistrées'],
    'Avisos': ['Alerts', 'Alertes'], 'Sonidos': ['Sounds', 'Sons'], 'Al guardar, al leer una etiqueta o cuando la ropa está lista.': ['When saving, scanning a label or when the laundry is ready.', "À l'enregistrement, à la lecture d'une étiquette ou quand le linge est prêt."],
    'Vibración': ['Vibration', 'Vibration'], 'Solo en móviles Android.': ['Android phones only.', 'Téléphones Android uniquement.'],
    'Mi lavadora': ['My washing machine', 'Ma machine à laver'], '¿Cuánto sueles tardar en tener la ropa lista?': ['How long does your laundry usually take?', 'Combien de temps te faut-il d’habitude pour avoir ton linge prêt ?'],
    'Lavar, tender y guardar. Saldrá marcado al poner la lavadora y siempre lo puedes cambiar.': ["Wash, hang and put away. It will be preselected when you start the laundry and you can always change it.", 'Laver, étendre et ranger. Ce sera présélectionné en lançant la machine et tu peux toujours le changer.'],
    '¿Cuándo pones la lavadora?': ['When do you do the laundry?', 'Quand lances-tu la machine ?'], 'Cuando quiera': ['Whenever I want', 'Quand je veux'],
    'Cada X días': ['Every X days', 'Tous les X jours'], 'Un día fijo': ['A fixed day', 'Un jour fixe'],
    'Pones la lavadora cuando quieras desde el cesto.': ['You start the laundry whenever you want from the basket.', 'Tu lances la machine quand tu veux depuis le panier.'],
    'Ese día te avisaré si tienes ropa en el cesto. La lavadora no se pone sola: tú la confirmas con un toque.': ["That day I'll remind you if there are clothes in the basket. The washer doesn't start by itself: you confirm with a tap.", "Ce jour-là, je te préviendrai s'il y a du linge dans le panier. La machine ne se lance pas toute seule : tu confirmes d'un geste."],
    'Pruebas para la presentación': ['Tests for the presentation', 'Tests pour la présentation'],
    'Herramientas para enseñar la app sin esperar días. No hacen falta para usar Cora.': ["Tools to demo the app without waiting days. You don't need them to use Cora.", "Outils pour montrer l'appli sans attendre des jours. Pas nécessaires pour utiliser Cora."],
    'Espera de la IA (reto 3)': ['AI waiting screen (challenge 3)', "Attente de l'IA (défi 3)"], 'Con mensajes': ['With messages', 'Avec messages'], 'Clásica': ['Classic', 'Classique'],
    'Cambia cómo se ve la espera del planificador para comparar con usuarios cuál parece más rápida.': ['Changes how the planner wait looks, to compare with users which one feels faster.', "Change l'affichage de l'attente du planificateur pour comparer avec des utilisateurs laquelle semble la plus rapide."],
    'Adelantar la lavadora': ['Fast-forward the laundry', 'Avancer la lessive'], '+12 horas': ['+12 hours', '+12 heures'],
    'Para ver cómo avanza la barra y cómo llega el aviso de «ya está seca».': ['To see how the bar moves and how the “it’s dry” alert arrives.', "Pour voir avancer la barre et arriver l'alerte « c'est sec »."],
    'Ahora no hay nada en la lavadora: pon una desde el cesto.': ['There is nothing in the washer now: start one from the basket.', "Il n'y a rien dans la machine : lance-en une depuis le panier."],
    'Datos de ejemplo': ['Sample data', "Données d'exemple"], 'Restablecer datos de ejemplo': ['Reset sample data', "Réinitialiser les données d'exemple"],
    'Restablecer': ['Reset', 'Réinitialiser'], 'Datos de ejemplo restablecidos': ['Sample data restored', "Données d'exemple restaurées"],
    'Se borrarán tus prendas, planes y guardados y volverán los datos de ejemplo.': ['Your clothes, plans and saved items will be deleted and the sample data will return.', 'Tes vêtements, projets et favoris seront supprimés et les données d’exemple reviendront.'],
    'Cerrar sesión': ['Log out', 'Se déconnecter'], 'Simular': ['Simulate', 'Simuler'], 'Generar': ['Generate', 'Générer'],

    // Cesto y lavadora
    '¿Cómo funciona?': ['How does it work?', 'Comment ça marche ?'], 'Ropa sucia al cesto.': ['Dirty clothes in the basket.', 'Linge sale au panier.'],
    'Abre la prenda y pulsa «Echar al cesto». Mientras está aquí, Cora no te la propone.': ['Open the item and tap “Put in the basket”. While it’s here, Cora won’t suggest it.', 'Ouvre le vêtement et touche « Mettre au panier ». Tant qu’il est ici, Cora ne le propose pas.'],
    'Pones la lavadora.': ['You start the washer.', 'Tu lances la machine.'],
    'Cuando la pongas de verdad, pulsa «Poner la lavadora»: va todo lo del cesto a la vez.': ['When you actually start it, tap “Start the washer”: everything in the basket goes at once.', 'Quand tu la lances vraiment, touche « Lancer la machine » : tout le panier part en même temps.'],
    'Te aviso cuando esté lista.': ["I'll let you know when it's ready.", 'Je te préviens quand c’est prêt.'],
    'Dices cuántos días tardas en lavarla y secarla, y ese día Cora te avisa para que vuelva a tu armario.': ['You say how many days it takes to wash and dry, and that day Cora reminds you so it goes back to your wardrobe.', 'Tu indiques combien de jours il faut pour laver et sécher, et ce jour-là Cora te prévient pour qu’il retourne dans ta garde-robe.'],
    'El cesto está vacío. Cuando una prenda esté sucia, ábrela y pulsa «Echar al cesto».': ['The basket is empty. When an item is dirty, open it and tap “Put in the basket”.', 'Le panier est vide. Quand un vêtement est sale, ouvre-le et touche « Mettre au panier ».'],
    'Hoy toca poner la lavadora según tu programación.': ["Today is laundry day according to your schedule.", "Aujourd'hui, c'est lessive selon ton programme."],
    '¿Cuándo estará lista para ponértela?': ['When will it be ready to wear?', 'Quand sera-t-il prêt à porter ?'],
    'Cuenta lo que tardas en lavarla, tenderla y que se seque (mínimo 2 días). Lo de siempre se cambia en Configuración.': ['Count how long it takes to wash, hang and dry (at least 2 days). Your usual time is changed in Settings.', 'Compte le temps pour laver, étendre et sécher (2 jours minimum). Ton temps habituel se change dans Paramètres.'],
    'Poner la lavadora': ['Start the washer', 'Lancer la machine'], 'Sacar alguna prenda': ['Take an item out', 'Retirer un vêtement'], 'Sacar': ['Take out', 'Retirer'],
    '1 semana': ['1 week', '1 semaine'], 'Fuera del cesto: disponible de nuevo': ['Out of the basket: available again', 'Hors du panier : de nouveau disponible'],
    'Prenda fuera del cesto: disponible de nuevo': ['Item out of the basket: available again', 'Vêtement hors du panier : de nouveau disponible'],
    'Márcala como disponible en Cora para que vuelva a tu armario.': ['Mark it as available in Cora so it goes back to your wardrobe.', 'Marque-le comme disponible dans Cora pour qu’il retourne dans ta garde-robe.'],

    // IA y tiempo
    'La IA solo funciona en la web publicada en Vercel; aquí uso mis reglas.': ['The AI only works on the site published on Vercel; here I use my rules.', "L'IA ne fonctionne que sur le site publié sur Vercel ; ici j'utilise mes règles."],
    'Falta configurar la clave de la IA en Vercel; de momento uso mis reglas.': ["The AI key isn't set up on Vercel yet; for now I use my rules.", "La clé de l'IA n'est pas configurée sur Vercel ; pour l'instant j'utilise mes règles."],
    'La IA ha llegado a su límite gratuito de hoy; uso mis reglas.': ['The AI has reached its free limit for today; I use my rules.', "L'IA a atteint sa limite gratuite du jour ; j'utilise mes règles."],
    'La IA ha tardado demasiado; uso mis reglas.': ['The AI took too long; I use my rules.', "L'IA a mis trop de temps ; j'utilise mes règles."],
    'No he podido hablar con la IA; uso mis reglas.': ["I couldn't reach the AI; I use my rules.", "Je n'ai pas pu joindre l'IA ; j'utilise mes règles."],
    'Añade más prendas para que la IA pueda combinarlas.': ['Add more items so the AI can combine them.', "Ajoute plus de vêtements pour que l'IA puisse les combiner."],
    'Previsión de ejemplo': ['Sample forecast', 'Prévisions d’exemple'], 'Previsión de ejemplo (sin conexión)': ['Sample forecast (offline)', 'Prévisions d’exemple (hors ligne)'],
    'La tienda no me ha dejado descargar la foto.': ["The shop didn't let me download the photo.", "La boutique ne m'a pas laissé télécharger la photo."],
    'No he encontrado la foto en la página de la tienda.': ["I couldn't find the photo on the shop's page.", "Je n'ai pas trouvé la photo sur la page de la boutique."],
    'La tienda ha tardado demasiado en responder.': ['The shop took too long to respond.', 'La boutique a mis trop de temps à répondre.'],
    'No he podido descargar la foto de la tienda.': ["I couldn't download the shop's photo.", "Je n'ai pas pu télécharger la photo de la boutique."],
    'La foto de la tienda solo se descarga en la web publicada en Vercel.': ["The shop photo only downloads on the site published on Vercel.", 'La photo de la boutique ne se télécharge que sur le site publié sur Vercel.'],

    // Estilos y gustos
    'Casual': ['Casual', 'Décontracté'], 'Minimalista': ['Minimalist', 'Minimaliste'], 'Clásico': ['Classic', 'Classique'], 'Deportivo': ['Sporty', 'Sportif'],
    'Vestidos': ['Dresses', 'Robes'], 'Chándal': ['Tracksuit', 'Survêtement'],

    // Prendas de ejemplo
    'Camiseta blanca': ['White T-shirt', 'T-shirt blanc'], 'Top de punto gris': ['Grey knit top', 'Top en maille gris'], 'Top rojo satinado': ['Red satin top', 'Top rouge satiné'],
    'Sudadera gris': ['Grey sweatshirt', 'Sweat gris'], 'Chaqueta de punto negra': ['Black cardigan', 'Gilet noir'], 'Blazer negro': ['Black blazer', 'Blazer noir'],
    'Vaquero azul': ['Blue jeans', 'Jean bleu'], 'Vaquero claro': ['Light jeans', 'Jean clair'], 'Falda negra': ['Black skirt', 'Jupe noire'],
    'Pantalón de chándal': ['Tracksuit bottoms', 'Pantalon de survêtement'], 'Pantalón cargo': ['Cargo trousers', 'Pantalon cargo'],
    'Zapatillas blancas': ['White sneakers', 'Baskets blanches'], 'Tacones negros': ['Black heels', 'Talons noirs'], 'Botas altas': ['Tall boots', 'Bottes hautes'],
    'Jersey gris oversize': ['Oversized grey sweater', 'Pull gris oversize'], 'Camisa azul cielo': ['Sky blue shirt', 'Chemise bleu ciel'],
    'Top halter satinado negro': ['Black satin halter top', 'Top dos nu satiné noir'], 'Top halter satinado caqui': ['Khaki satin halter top', 'Top dos nu satiné kaki'],
    'Body plisado burdeos': ['Burgundy pleated bodysuit', 'Body plissé bordeaux'], 'Top palabra de honor rojo': ['Red strapless top', 'Top bustier rouge'],
    'Top asimétrico drapeado': ['Draped asymmetric top', 'Top asymétrique drapé'], 'Camiseta de punto con volantes': ['Ruffled knit top', 'T-shirt en maille à volants'],
    'Vaquero recto negro': ['Black straight jeans', 'Jean droit noir'], 'Vaquero ancho azul': ['Blue wide-leg jeans', 'Jean large bleu'],
    'Vaquero recto crudo': ['Ecru straight jeans', 'Jean droit écru'], 'Chaqueta de ante con borrego': ['Suede shearling jacket', 'Veste en suède et mouton'],
    'Chaqueta peplum caqui': ['Khaki peplum jacket', 'Veste péplum kaki'], 'Chaqueta de piel con cinturón': ['Belted leather jacket', 'Veste en cuir ceinturée'],
    'Jersey amarillo pastel': ['Pastel yellow sweater', 'Pull jaune pastel'], 'Jersey de pico azul marino': ['Navy V-neck sweater', 'Pull col V bleu marine'],
    'Jersey polo de rayas': ['Striped polo sweater', 'Pull polo rayé'], 'Camiseta de manga larga gris': ['Grey long-sleeve top', 'T-shirt manches longues gris'],
    'Sudadera con cremallera coral': ['Coral zip-up sweatshirt', 'Sweat zippé corail'], 'Jogger caqui': ['Khaki joggers', 'Jogging kaki'],
    'Jogger coral': ['Coral joggers', 'Jogging corail'],
    'Jersey de punto marrón': ['Brown knit sweater', 'Pull en maille marron'], 'Jersey con cremallera amarillo': ['Yellow half-zip sweater', 'Pull zippé jaune'],
    'Polo de punto de rayas': ['Striped knit polo', 'Polo en maille rayé'], 'Camiseta blanca básica': ['Basic white T-shirt', 'T-shirt blanc basique'],
    'Camiseta de rayas azul': ['Blue striped T-shirt', 'T-shirt rayé bleu'], 'Camiseta de rayas verde': ['Green striped T-shirt', 'T-shirt rayé vert'],
    'Camiseta oversize granate': ['Maroon oversized T-shirt', 'T-shirt oversize bordeaux'], 'Jogger gris oscuro': ['Dark grey joggers', 'Jogging gris foncé'],
    'Pantalón cargo jogger': ['Cargo joggers', 'Jogging cargo'], 'Pantalón recto gris azulado': ['Blue-grey straight trousers', 'Pantalon droit gris-bleu'],
    'Zapatillas retro blancas': ['White retro sneakers', 'Baskets rétro blanches'],
    'Camiseta de manga larga gris lavado': ['Washed grey long-sleeve T-shirt', 'T-shirt manches longues gris délavé'], 'Pantalón corto de deporte': ['Sports shorts', 'Short de sport'],
    'Jogger azul marino': ['Navy joggers', 'Jogging bleu marine'], 'Pantalón técnico negro': ['Black technical trousers', 'Pantalon technique noir'],
    'Plumífero negro': ['Black puffer jacket', 'Doudoune noire'], 'Chaqueta polar gris': ['Grey fleece jacket', 'Veste polaire grise'],
    'Chaqueta técnica con capucha': ['Hooded technical jacket', 'Veste technique à capuche'], 'Chaqueta de running negra': ['Black running jacket', 'Veste de running noire'],
    'Chaqueta impermeable lima': ['Lime rain jacket', 'Veste imperméable citron vert'], 'Zapatillas de ante caqui': ['Khaki suede sneakers', 'Baskets en daim kaki'], 'Pantalón ancho de felpa gris': ['Grey wide-leg sweatpants', 'Pantalon large molletonné gris'],
  };

  // Palabras sueltas que aparecen dentro de textos con números o fechas
  const DIAS = { domingo: ['Sunday', 'dimanche'], lunes: ['Monday', 'lundi'], martes: ['Tuesday', 'mardi'], 'miércoles': ['Wednesday', 'mercredi'], jueves: ['Thursday', 'jeudi'], viernes: ['Friday', 'vendredi'], 'sábado': ['Saturday', 'samedi'] };
  const MESES = { enero: ['January', 'janvier'], febrero: ['February', 'février'], marzo: ['March', 'mars'], abril: ['April', 'avril'], mayo: ['May', 'mai'], junio: ['June', 'juin'], julio: ['July', 'juillet'], agosto: ['August', 'août'], septiembre: ['September', 'septembre'], octubre: ['October', 'octobre'], noviembre: ['November', 'novembre'], diciembre: ['December', 'décembre'] };
  const CIELO = { sol: ['sunny', 'soleil'], nubes: ['some clouds', 'nuageux'], nublado: ['cloudy', 'couvert'], lluvia: ['rain', 'pluie'] };
  const CORTOS = { L: ['M', 'L'], M: ['T', 'M'], X: ['W', 'M'], J: ['T', 'J'], V: ['F', 'V'], S: ['S', 'S'], D: ['S', 'D'] };

  let i = 0; // 0 inglés, 1 francés
  const cap = s => s.charAt(0).toUpperCase() + s.slice(1);
  const palabra = (dic, w) => { const k = w.toLowerCase(); if (!dic[k]) return w; const r = dic[k][i]; return w[0] === w[0].toUpperCase() ? cap(r) : r; };
  const dia = w => palabra(DIAS, w), mes = w => palabra(MESES, w), cielo = w => palabra(CIELO, w);
  const n = (num, uno, varios) => num + ' ' + (+num === 1 ? uno[i] : varios[i]);

  // Textos que llevan números, nombres o fechas: expresión regular → traducción
  const P = [
    [/^Hoy$/, () => ['Today', "Aujourd'hui"][i]], [/^Mañana$/, () => ['Tomorrow', 'Demain'][i]], [/^Ayer$/, () => ['Yesterday', 'Hier'][i]],
    [/^Mañana · $/, () => ['Tomorrow · ', 'Demain · '][i]],
    [/^(Lunes|Martes|Miércoles|Jueves|Viernes|Sábado|Domingo) (\d+)$/, (m) => i === 0 ? `${dia(m[1])} ${m[2]}` : `${dia(m[1])} ${m[2]}`],
    [/^(lunes|martes|miércoles|jueves|viernes|sábado|domingo), (\d+) de (\w+)$/i, (m) => i === 0 ? `${dia(m[1])}, ${mes(m[3])} ${m[2]}` : `${dia(m[1])} ${m[2]} ${mes(m[3])}`],
    [/^(Lunes|Martes|Miércoles|Jueves|Viernes|Sábado|Domingo), (\d+) de (\w+) · (-?\d+)° (\w+)$/, (m) => `${i === 0 ? `${dia(m[1])}, ${mes(m[3])} ${m[2]}` : `${dia(m[1])} ${m[2]} ${mes(m[3])}`} · ${m[4]}° ${cielo(m[5])}`],
    [/^(Enero|Febrero|Marzo|Abril|Mayo|Junio|Julio|Agosto|Septiembre|Octubre|Noviembre|Diciembre) (\d{4})$/, (m) => `${cap(mes(m[1]))} ${m[2]}`],
    [/^(\d+) (\w{3}) – (\d+) (\w{3})$/, (m) => { const c = w => { const k = Object.keys(MESES).find(x => x.startsWith(w)); return k ? MESES[k][i].slice(0, 3) : w; }; return `${m[1]} ${c(m[2])} – ${m[3]} ${c(m[4])}`; }],
    [/^(-?\d+)° (sol|nubes|nublado|lluvia)$/, (m) => `${m[1]}° ${cielo(m[2])}`],
    [/^(-?\d+)° · (sol|nubes|nublado|lluvia)$/, (m) => `${m[1]}° · ${cielo(m[2])}`],
    [/^(-?\d+)°, (sol|nubes|nublado|lluvia)$/, (m) => `${m[1]}°, ${cielo(m[2])}`],
    [/^Previsión real de (.+)$/, (m) => [`Real forecast for ${m[1]}`, `Prévisions réelles pour ${m[1]}`][i]],
    [/^Previsión de ejemplo \(no encuentro «(.+)»\)$/, (m) => [`Sample forecast (can't find “${m[1]}”)`, `Prévisions d’exemple (« ${m[1]} » introuvable)`][i]],
    [/^(\d+) prendas?$/, (m) => n(m[1], ['item', 'vêtement'], ['items', 'vêtements'])],
    [/^(\d+) prendas esperando a la lavadora$/, (m) => [`${m[1]} items waiting for the washer`, `${m[1]} vêtements attendent la machine`][i]],
    [/^1 prenda esperando a la lavadora$/, () => ['1 item waiting for the washer', '1 vêtement attend la machine'][i]],
    [/^(\d+) días$/, (m) => [`${m[1]} days`, `${m[1]} jours`][i]], [/^Cada (\d+) días$/, (m) => [`Every ${m[1]} days`, `Tous les ${m[1]} jours`][i]],
    [/^hace (\d+) min$/, (m) => [`${m[1]} min ago`, `il y a ${m[1]} min`][i]], [/^hace (\d+) h$/, (m) => [`${m[1]} h ago`, `il y a ${m[1]} h`][i]],
    [/^hace (\d+) días$/, (m) => [`${m[1]} days ago`, `il y a ${m[1]} jours`][i]],
    [/^No encontramos «(.+)»\. Revisa cómo está escrita\.$/, (m) => [`We can't find “${m[1]}”. Check the spelling.`, `Nous ne trouvons pas « ${m[1]} ». Vérifie l'orthographe.`][i]],
    [/^No hay resultados para «(.+)»\. Prueba con un nombre o con una prenda, por ejemplo «vaquero»\.$/, (m) => [`No results for “${m[1]}”. Try a name or an item, for example “jeans”.`, `Aucun résultat pour « ${m[1]} ». Essaie un nom ou un vêtement, par exemple « jean ».`][i]],
    [/^Seguir a (.+)$/, (m) => [`Follow ${m[1]}`, `Suivre ${m[1]}`][i]], [/^Outfit de (.+)$/, (m) => [`${m[1]}'s outfit`, `Tenue de ${m[1]}`][i]],
    [/^Ver el perfil de (.+)$/, (m) => [`See ${m[1]}'s profile`, `Voir le profil de ${m[1]}`][i]],
    [/^Comentarios \((\d+)\)$/, (m) => [`Comments (${m[1]})`, `Commentaires (${m[1]})`][i]],
    [/^Mis outfits \((\d+)\)$/, (m) => [`My outfits (${m[1]})`, `Mes tenues (${m[1]})`][i]], [/^De Explorar \((\d+)\)$/, (m) => [`From Explore (${m[1]})`, `D'Explorer (${m[1]})`][i]],
    [/^Outfits \((\d+)\)$/, (m) => [`Outfits (${m[1]})`, `Tenues (${m[1]})`][i]], [/^Prendas favoritas \((\d+)\)$/, (m) => [`Favourite items (${m[1]})`, `Vêtements favoris (${m[1]})`][i]],
    [/^Sacar del cesto \((\d+)\)$/, (m) => [`Take out of basket (${m[1]})`, `Sortir du panier (${m[1]})`][i]],
    [/^Poner la lavadora \((\d+) prendas?\)$/, (m) => [`Start the washer (${m[1]} ${+m[1] === 1 ? 'item' : 'items'})`, `Lancer la machine (${m[1]} ${+m[1] === 1 ? 'vêtement' : 'vêtements'})`][i]],
    [/^Preparar la semana \((\d+) días con planes\)$/, (m) => [`Prepare the week (${m[1]} days with plans)`, `Préparer la semaine (${m[1]} jours avec projets)`][i]],
    [/^Preparar el outfit de (hoy|mañana|el .+)$/, (m) => [`Prepare the outfit for ${cuando(m[1])}`, `Préparer la tenue de ${cuando(m[1])}`][i]],
    [/^Preparar otro outfit para (hoy|mañana|el .+)$/, (m) => [`Prepare another outfit for ${cuando(m[1])}`, `Préparer une autre tenue pour ${cuando(m[1])}`][i]],
    [/^Cuéntale a Cora qué vas a hacer (hoy|mañana|el .+)\.$/, (m) => [`Tell Cora what you're doing ${cuando(m[1])}.`, `Raconte à Cora ce que tu fais ${cuando(m[1])}.`][i]],
    [/^(Hoy|Mañana|Ayer|Lunes \d+|Martes \d+|Miércoles \d+|Jueves \d+|Viernes \d+|Sábado \d+|Domingo \d+) · (.+)$/, (m) => `${traducir(m[1]) ?? m[1]} · ${traducir(m[2]) ?? m[2]}`],
    [/^(.+) ya está seca$/, (m) => [`${m[1]} is dry`, `${m[1]} est sec`][i]], [/^(\d+) prendas ya están secas$/, (m) => [`${m[1]} items are dry`, `${m[1]} vêtements sont secs`][i]],
    [/^(.+) guardada en tu armario$/, (m) => [`${m[1]} saved to your wardrobe`, `${m[1]} enregistré dans ta garde-robe`][i]],
    [/^Lavadora puesta con (\d+) prendas?\. Te avisaré cuando esté lista\.$/, (m) => [`Washer started with ${m[1]} item(s). I'll let you know when it's ready.`, `Machine lancée avec ${m[1]} vêtement(s). Je te préviens quand c'est prêt.`][i]],
    [/^Prenda de (.+)$/, (m) => [`${m[1]} item`, `Vêtement ${m[1]}`][i]],
    [/^Pones la lavadora cada (\d+) días(?: · (toca hoy|la próxima, el (\S+) (\d+)))?$/, (m) => [`You do laundry every ${m[1]} days`, `Tu fais la lessive tous les ${m[1]} jours`][i]
      + (!m[2] ? '' : m[2] === 'toca hoy' ? [' · today', " · aujourd'hui"][i] : [` · next: ${dia(m[3])} ${m[4]}`, ` · prochaine : ${dia(m[3])} ${m[4]}`][i])],
    [/^Sueles poner la lavadora los (\S+)$/, (m) => { const d = DIAS[m[1].toLowerCase()] ? m[1] : m[1].replace(/s$/, ''); return [`You usually do laundry on ${cap(dia(d))}s`, `Tu fais d'habitude la lessive le ${dia(d)}`][i]; }],
  ];
  function cuando(t) {
    if (t === 'hoy') return ['today', "aujourd'hui"][i];
    if (t === 'mañana') return ['tomorrow', 'demain'][i];
    const m = t.match(/^el (\w+) (\d+)$/i); return m ? (i === 0 ? `${dia(m[1])} ${m[2]}` : `${dia(m[1])} ${m[2]}`) : t;
  }

  function traducir(t) {
    const r = T[t]; if (r) return r[i];
    for (const [re, f] of P) { const m = t.match(re); if (m) return f(m); }
    return null;
  }

  // ---------------------------------------------------------------- aplicar a la página
  let idioma = 'es';
  const original = new WeakMap(), puesto = new WeakMap();   // texto en español y el traducido que puse yo
  const ATRIBUTOS = ['placeholder', 'aria-label', 'title', 'alt'];

  function textoNodo(nodo) {
    const v = nodo.nodeValue;
    if (puesto.get(nodo) === v) return;            // lo cambié yo
    if (!v || !v.trim()) return;
    original.set(nodo, v);                          // Blazor ha puesto un texto nuevo (en español)
    if (idioma === 'es') return;
    const t = v.trim();
    // Iniciales de los días del calendario (L M X J V S D): solo dentro de la cabecera del mes, para no tocar iniciales de nombres
    const tr = nodo.parentElement && nodo.parentElement.classList.contains('dn') && CORTOS[t] ? CORTOS[t][i] : traducir(t);
    if (tr != null && tr !== t) { const nv = v.replace(t, tr); puesto.set(nodo, nv); nodo.nodeValue = nv; }
  }
  function atributos(el) {
    for (const a of ATRIBUTOS) {
      if (!el.hasAttribute || !el.hasAttribute(a)) continue;
      const clave = 'data-es-' + a, v = el.getAttribute(a), mio = el.getAttribute('data-tr-' + a);
      if (mio === v) continue;
      el.setAttribute(clave, v);
      if (idioma === 'es') continue;
      const tr = traducir(v.trim());
      if (tr != null) { el.setAttribute('data-tr-' + a, tr); el.setAttribute(a, tr); }
    }
  }
  function recorrer(raiz) {
    if (raiz.nodeType === 3) { textoNodo(raiz); return; }
    if (raiz.nodeType !== 1 || raiz.closest('[data-sin-traducir], script, style, textarea')) return;
    atributos(raiz);
    const w = document.createTreeWalker(raiz, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, {
      acceptNode: n => n.nodeType === 1 && n.matches('[data-sin-traducir], script, style, textarea') ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT,
    });
    let n2; while ((n2 = w.nextNode())) { if (n2.nodeType === 3) textoNodo(n2); else atributos(n2); }
  }

  const obs = new MutationObserver(lista => {
    for (const m of lista) {
      if (m.type === 'characterData') textoNodo(m.target);
      else if (m.type === 'attributes') atributos(m.target);
      else m.addedNodes.forEach(recorrer);
    }
  });

  function poner(nuevo) {
    nuevo = ['en', 'fr'].includes(nuevo) ? nuevo : 'es';
    if (nuevo === idioma) return;
    idioma = nuevo; i = nuevo === 'fr' ? 1 : 0;
    document.documentElement.lang = nuevo;
    // Primero se vuelve todo al español original y luego se traduce al idioma nuevo
    const w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    let t; while ((t = w.nextNode())) { const o = original.get(t); if (o != null && t.nodeValue !== o) { puesto.set(t, o); t.nodeValue = o; } }
    document.querySelectorAll('[data-es-placeholder],[data-es-aria-label],[data-es-title],[data-es-alt]').forEach(el => {
      for (const a of ATRIBUTOS) { const o = el.getAttribute('data-es-' + a); if (o != null) { el.setAttribute('data-tr-' + a, o); el.setAttribute(a, o); } }
    });
    if (idioma !== 'es') { const w2 = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT); let x; while ((x = w2.nextNode())) { puesto.delete(x); textoNodo(x); } document.querySelectorAll('*').forEach(el => { ATRIBUTOS.forEach(a => el.removeAttribute('data-tr-' + a)); atributos(el); }); }
  }

  function iniciar() {
    recorrer(document.body);
    obs.observe(document.body, { subtree: true, childList: true, characterData: true, attributes: true, attributeFilter: ATRIBUTOS });
  }
  if (document.body && document.body.nodeType) iniciar(); else document.addEventListener('DOMContentLoaded', iniciar);

  return { poner: poner, traducir: t => (idioma === 'es' ? t : traducir(t) ?? t) };
})();
