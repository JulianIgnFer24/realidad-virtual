/* ============================================================================
   TP4 - Realidad Virtual - Ejercicio 1
   Ambiente 3D en Processing con un conjunto de planos que rotan.

   FIGURA: una ESFERA atravesada por DOS PLANOS PERPENDICULARES entre si que
   se cruzan justo en el centro de la esfera.
     - El plano A ocupa el plano XY (su normal apunta sobre el eje Z).
     - El plano B esta girado 90 grados sobre el eje Y, o sea que ocupa el
       plano YZ (su normal apunta sobre el eje X).
   Los tres cuerpos giran como un unico conjunto.

   CONTROLES
     FLECHA IZQ / DER : rotar el conjunto sobre el eje VERTICAL   (sentido - / +)
     FLECHA ARR / ABA : rotar el conjunto sobre el eje HORIZONTAL (sentido - / +)
     D                : detener la animacion
     R                : reiniciar la animacion (vuelve a la posicion inicial)

   DISENO OO: cada responsabilidad vive en su propia clase.
     ControlRotacion -> estado de la animacion (eje, sentido, velocidad)
     Damero          -> fabrica la textura de cuadros de cada superficie
     Plano           -> una superficie plana: su orientacion y su textura
     Esfera          -> el cuerpo esferico central y su textura
     Figura          -> arma el conjunto y le aplica la rotacion global
     Iluminacion     -> las luces de la escena
   ========================================================================== */


// ---------------------------------------------------------------------------
// CONSTANTES DE LA FIGURA
// ---------------------------------------------------------------------------

final float RADIO_ESFERA   = 110;  // radio de la esfera central
final float LADO_PLANO     = 340;  // lado de cada plano cuadrado
final int   DETALLE_ESFERA = 32;   // cuantos segmentos tiene la malla de la esfera

// Inclinacion fija de la camara, para mirar la figura en perspectiva y que se
// entienda que es 3D incluso cuando la animacion esta detenida.
final float VISTA_X = -0.42;
final float VISTA_Y = -0.55;


// ---------------------------------------------------------------------------
// OBJETOS PRINCIPALES DE LA ESCENA
// ---------------------------------------------------------------------------

Figura          figura;      // la esfera junto con los dos planos
ControlRotacion control;     // el estado de la animacion
Iluminacion     iluminacion; // las luces


void setup() {
  size(900, 700, P3D);       // ventana con renderer 3D (necesario para rotateX/Y y luces)
  textureMode(NORMAL);       // las coordenadas de textura van de 0 a 1 (no en pixeles)
  noStroke();                // sin contorno: las superficies se definen por su relleno y la luz

  control     = new ControlRotacion();  // arranca girando sobre el eje vertical
  iluminacion = new Iluminacion();      // arma el esquema de luces
  figura      = new Figura();           // construye la esfera, los planos y sus texturas
}


void draw() {
  background(18, 20, 30);    // fondo oscuro: hace resaltar las superficies iluminadas

  iluminacion.aplicar();     // las luces se declaran en cada frame, antes de dibujar

  control.avanzar();         // suma el paso de rotacion de este frame (si esta activa)

  pushMatrix();              // guardo el sistema de coordenadas original
  translate(width/2, height/2, 0); // muevo el origen al centro de la ventana
  rotateX(VISTA_X);          // inclino la vista para ver la figura en perspectiva
  rotateY(VISTA_Y);
  figura.dibujar(control);   // dibujo la figura aplicando la rotacion animada
  popMatrix();               // restauro el sistema de coordenadas

  dibujarHUD();              // encima de todo, el cartel con el estado y los controles
}


// ---------------------------------------------------------------------------
// ENTRADA DEL USUARIO
// Solo traduce la tecla a un mensaje: la logica vive en las clases.
// ---------------------------------------------------------------------------

void keyPressed() {
  if (key == CODED) {                       // las flechas son teclas "codificadas"
    if (keyCode == LEFT)  control.aplicarEje(ControlRotacion.EJE_VERTICAL,   -1);
    if (keyCode == RIGHT) control.aplicarEje(ControlRotacion.EJE_VERTICAL,   +1);
    if (keyCode == UP)    control.aplicarEje(ControlRotacion.EJE_HORIZONTAL, -1);
    if (keyCode == DOWN)  control.aplicarEje(ControlRotacion.EJE_HORIZONTAL, +1);
  } else {
    if (key == 'd' || key == 'D') control.detener();    // D: detener la animacion
    if (key == 'r' || key == 'R') control.reiniciar();  // R: volver a la posicion inicial
  }
}


// ---------------------------------------------------------------------------
// HUD: texto 2D sobre la escena 3D
// ---------------------------------------------------------------------------

void dibujarHUD() {
  hint(DISABLE_DEPTH_TEST);  // el texto se dibuja siempre adelante, sin test de profundidad
  noLights();                // el texto no debe verse afectado por las luces de la escena
  camera();                  // vuelvo a la camara por defecto: el plano z=0 mapea 1:1 a pixeles

  fill(235);                 // color del texto
  textAlign(LEFT, TOP);
  textSize(14);
  text("Flechas IZQ/DER : giro sobre eje VERTICAL", 18, 16);
  text("Flechas ARR/ABA : giro sobre eje HORIZONTAL", 18, 34);
  text("D : detener     R : reiniciar posicion", 18, 52);

  fill(120, 220, 255);       // el estado actual, en otro color para distinguirlo
  text("Eje: "     + control.nombreEje() +
       "   Sentido: " + control.nombreSentido() +
       "   Estado: "  + (control.estaActiva() ? "en movimiento" : "detenida"), 18, 78);

  hint(ENABLE_DEPTH_TEST);   // restauro el test de profundidad para el proximo frame
}


/* ==========================================================================
   CLASE ControlRotacion
   Guarda el estado de la animacion: sobre que eje se gira, en que sentido,
   a que velocidad, y cuanto se lleva girado hasta ahora.
   ========================================================================== */
class ControlRotacion {

  static final int EJE_VERTICAL   = 0;  // eje Y: la figura gira como una calesita
  static final int EJE_HORIZONTAL = 1;  // eje X: la figura gira como una rueda

  int   eje       = EJE_VERTICAL;  // eje activo (solo uno a la vez, como pide la consigna)
  int   sentido   = +1;            // +1 horario / -1 antihorario
  float velocidad = 0.012;         // radianes que avanza por frame
  boolean activa  = true;          // si false, la figura queda congelada

  float anguloY = 0;               // angulo acumulado sobre el eje vertical
  float anguloX = 0;               // angulo acumulado sobre el eje horizontal

  // Una flecha elige el eje Y el sentido de una sola vez.
  // Si la animacion estaba detenida, volver a pulsar una flecha la reanuda.
  void aplicarEje(int nuevoEje, int nuevoSentido) {
    eje     = nuevoEje;
    sentido = nuevoSentido;
    activa  = true;
  }

  void detener() {
    activa = false;                // congela la figura conservando los angulos actuales
  }

  // Reinicia: vuelve a la posicion inicial y al estado de arranque.
  void reiniciar() {
    anguloX = 0;
    anguloY = 0;
    eje     = EJE_VERTICAL;
    sentido = +1;
    activa  = true;
  }

  // Suma el paso de este frame al angulo del eje activo.
  void avanzar() {
    if (!activa) return;           // detenida: no acumula nada
    if (eje == EJE_VERTICAL) {
      anguloY += velocidad * sentido;
    } else {
      anguloX += velocidad * sentido;
    }
  }

  boolean estaActiva()   { return activa; }
  String  nombreEje()    { return (eje == EJE_VERTICAL) ? "VERTICAL" : "HORIZONTAL"; }
  String  nombreSentido(){ return (sentido > 0) ? "+" : "-"; }
}


/* ==========================================================================
   CLASE Damero
   Fabrica la textura de las superficies dibujandola por codigo, asi el sketch
   no depende de imagenes externas. Hay un unico estilo: un damero de pocos
   cuadros grandes en dos colores. Cada superficie usa su propio par de colores.
   ========================================================================== */
class Damero {

  final int LADO     = 256;  // resolucion de la textura, en pixeles
  final int CUADROS  = 4;    // cuadros por lado (4x4: pocos y grandes)

  PGraphics crear(color claro, color oscuro) {
    PGraphics pg = createGraphics(LADO, LADO);
    int celda = LADO / CUADROS;

    pg.beginDraw();
    pg.noStroke();
    pg.background(oscuro);                       // los cuadros oscuros son el fondo
    pg.fill(claro);
    for (int i = 0; i < CUADROS; i++) {
      for (int j = 0; j < CUADROS; j++) {
        if ((i + j) % 2 == 0) {                  // pinto un cuadro claro uno por medio
          pg.rect(i*celda, j*celda, celda, celda);
        }
      }
    }
    pg.endDraw();
    return pg;
  }
}


/* ==========================================================================
   CLASE Plano
   Una de las dos superficies planas que atraviesan la esfera. Esta centrada
   en el origen (o sea, en el centro de la esfera) y solo se diferencia de la
   otra por su angulo de orientacion.
   ========================================================================== */
class Plano {

  float orientacion;    // giro sobre el eje Y que define como queda parado el plano
  float lado;           // los planos son cuadrados
  PGraphics textura;    // el damero que lleva pintado

  Plano(float orientacion, float lado, PGraphics textura) {
    this.orientacion = orientacion;
    this.lado        = lado;
    this.textura     = textura;
  }

  // Orienta el plano y lo dibuja. No hay translate: queda centrado en el
  // origen, que es justamente el centro de la esfera.
  void dibujar() {
    pushMatrix();                       // aislo las transformaciones de este plano
    rotateY(orientacion);               // 0 para el plano XY, 90 grados para el plano YZ

    // Se dibujan las dos caras con normales opuestas para que la iluminacion
    // funcione bien tanto de frente como de contraluz.
    dibujarCara(+1);
    dibujarCara(-1);

    popMatrix();                        // vuelvo al sistema de coordenadas de la figura
  }

  // Dibuja un lado del plano. 'cara' vale +1 (mira al frente) o -1 (mira al dorso).
  void dibujarCara(int cara) {
    float m = lado / 2;                 // medio lado: el cuadrilatero esta centrado en el origen

    beginShape();                       // modo poligono: 4 vertices = un cuadrilatero
    texture(textura);                   // uso el damero como imagen del cuadrilatero
    normal(0, 0, cara);                 // la normal define hacia donde "mira" la cara

    if (cara > 0) {                     // cara frontal: vertices en sentido horario
      vertex(-m, -m, 0, 0, 0);          // cada vertex lleva (x, y, z, u, v)
      vertex( m, -m, 0, 1, 0);
      vertex( m,  m, 0, 1, 1);
      vertex(-m,  m, 0, 0, 1);
    } else {                            // cara trasera: mismo cuadrilatero con el orden invertido
      vertex(-m, -m, 0, 0, 0);
      vertex(-m,  m, 0, 0, 1);
      vertex( m,  m, 0, 1, 1);
      vertex( m, -m, 0, 1, 0);
    }
    endShape(CLOSE);
  }
}


/* ==========================================================================
   CLASE Esfera
   El cuerpo central que los planos atraviesan. Se arma una sola vez como
   PShape (una malla reutilizable) para no recalcularla en cada frame.
   ========================================================================== */
class Esfera {

  PShape forma;         // la malla de la esfera, ya construida

  Esfera(float radio, PGraphics textura) {
    sphereDetail(DETALLE_ESFERA);       // mas segmentos = silueta mas redonda
    forma = createShape(SPHERE, radio); // Processing genera la malla con sus coordenadas de textura
    forma.setStroke(false);             // sin alambrado: solo la superficie
    forma.setTexture(textura);          // le aplico el damero
  }

  void dibujar() {
    shape(forma);                       // la esfera queda centrada en el origen
  }
}


/* ==========================================================================
   CLASE Figura
   El conjunto completo: la esfera mas los dos planos perpendiculares.
   Se encarga de crearlos, darles su textura y aplicarles la rotacion global
   que dicta el ControlRotacion.
   ========================================================================== */
class Figura {

  Esfera  esfera;                 // el cuerpo central
  Plano[] planos;                 // las dos superficies que lo atraviesan

  Figura() {
    Damero damero = new Damero();

    // Los dos planos se diferencian solo por su orientacion:
    //   0        -> queda sobre el plano XY
    //   HALF_PI  -> girado 90 grados, queda sobre el plano YZ
    // Cada superficie lleva el mismo damero pero con su propio color.
    planos = new Plano[2];
    planos[0] = new Plano(0,       LADO_PLANO, damero.crear(color(90, 160, 240), color(25, 50, 95)));  // azul
    planos[1] = new Plano(HALF_PI, LADO_PLANO, damero.crear(color(245, 150, 70), color(100, 45, 20))); // naranja

    esfera = new Esfera(RADIO_ESFERA, damero.crear(color(235, 235, 235), color(60, 60, 60)));        // gris
  }

  // Aplica la rotacion global acumulada y dibuja todo el conjunto.
  void dibujar(ControlRotacion control) {
    pushMatrix();
    rotateX(control.anguloX);       // giro sobre el eje horizontal
    rotateY(control.anguloY);       // giro sobre el eje vertical

    fill(255);                      // blanco: deja pasar el color real de la textura
    specular(255);                  // brillo especular para que la luz "resalte"
    shininess(12);                  // concentracion del brillo

    esfera.dibujar();
    for (int i = 0; i < planos.length; i++) {
      planos[i].dibujar();
    }

    popMatrix();
  }
}


/* ==========================================================================
   CLASE Iluminacion
   Luz ambiente tenue + direccional principal + una puntual. La ambiente evita
   que las caras en sombra queden negras, la direccional define el volumen (es
   la que redondea la esfera) y la puntual aporta el brillo especular.
   ========================================================================== */
class Iluminacion {

  void aplicar() {
    ambientLight(60, 60, 70);                                  // relleno suave general
    directionalLight(220, 215, 200, -0.5, 0.7, -0.6);          // luz principal: arriba, izquierda, frente
    directionalLight(90, 90, 110, 0.5, -0.7, 0.6);             // contraluz debil: que el dorso no quede negro
    pointLight(255, 230, 200, width*0.80, height*0.25, 420);   // brillo especular
  }
}
