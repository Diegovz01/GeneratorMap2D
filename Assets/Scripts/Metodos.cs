using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Metodos
{
    /// <summary>
    /// Genera un array Bidimensional
    /// </summary>
    /// <param name="ancho">Ancho del mapa en 2D</param>
    /// <param name="alto">Alto del mapa en 2D</param>
    /// <param name="vacio">Verdadero, si queremos inicializarlo todo a cero. Si no, todo se inicializara a 1</param>
    /// <returns>El mapa 2D generado.</returns>
    public static int[,] GenerarArray(int ancho, int alto, bool vacio)
    {
        int[,] mapa = new int[ancho, alto];
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // Esto es igual a...
                /*
                if (vacio)
                {
                    mapa[x, y] = 0;
                }
                else
                {
                    mapa[x, y] = 1;
                }
                */
                // Esto, es mas elegante.
                mapa[x, y] = vacio ? 0 : 1;
            }
        }
        return mapa;
    }

    /// <summary>
    /// Genera el mapa de losetas con la informacion indicada en "mapa".
    /// </summary>
    /// <param name="mapa">Información que se utilizara para generar el mapa de losetas. 1 = Hay losetas, 0 = No hay losteas.</param>
    /// <param name="mapaDeLosetas">Referencia al mapa de losetas donde se generán las losetas.</param>
    /// <param name="loseta">Loseta que se utilizará para pintar el suelo en el mapa de losetas.</param>
    public static void GenerarMapa(int[,] mapa, Tilemap mapaDeLosetas, TileBase loseta)
    {
        // Limpiamos el mapa de casillas para empezar con uno vacio.
        mapaDeLosetas.ClearAllTiles();

        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= mapa.GetUpperBound(1); y++)
            {
                // 1 = Hay suelo, 0 = No hay suelo
                if (mapa[x, y] == 1)
                {
                    mapaDeLosetas.SetTile(new Vector3Int(x, y, 0), loseta);
                }
            }
        }
    }

    /// <summary>
    /// Genera el terreno basandose en el Perlin Noise.
    /// </summary>
    /// <param name="mapa">EL array a modificar donde se guarda el terreno generado.</param>
    /// <param name="semilla">La semilla que se utilizará para generar el terreno.</param>
    /// <returns>El array modificado con el terreno generado.</returns>
    public static int[,] PerlinNoise(int[,] mapa, float semilla)
    {
        // La altura actual del punto en x.
        int nuevopunto;

        // Como Mathf.PerlinNoise devuelve un valor entre 0 y 1, le restamos
        // esta variable para que el valor final este entre -0.5 y o.5.
        float reduccion = 0.5f;
        // Crear el PerlinNoise
        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            nuevopunto = Mathf.FloorToInt((Mathf.PerlinNoise(x, semilla) - reduccion) * mapa.GetUpperBound(1));
            nuevopunto += (mapa.GetUpperBound(1) / 2);
            for (int y = nuevopunto; y >= 0; y--)
            {
                mapa[x, y] = 1;
            }
        }

        return mapa;
    }

    /// <summary>
    /// Modifica el mapa creando un terreno usando Perlin Noise Suavisado.
    /// </summary>
    /// <param name="mapa">El mapa que vamos a editar.</param>
    /// <param name="semilla">La semilla para la generacion del Perlin.</param>
    /// <param name="intervalo">El intervalo en el que grabamos la altura.</param>
    /// <returns>El mapa modificado</returns>
    public static int[,] PerlinNoiseSuavisado(int[,] mapa, float semilla, int intervalo)
    {
        if (intervalo > 1)
        {
            // Utilizados en el proceso de suavisado
            Vector2Int posicionActual, posicionAnterior;

            // Los puntos correspondientes para el suavisado. Una lista para cada eje.
            List<int> ruidoX = new List<int>();
            List<int> ruidoY = new List<int>();

            int nuevoPunto, puntos;

            // Genera el ruido.
            for (int x = 0; x <= mapa.GetUpperBound(0) + intervalo; x += intervalo)
            {
                nuevoPunto = Mathf.FloorToInt(Mathf.PerlinNoise(x, semilla) * mapa.GetUpperBound(1));
                ruidoY.Add(nuevoPunto);
                ruidoX.Add(x);
            }
            puntos = ruidoY.Count;

            // Empezamos en la primera posición para así tener disponible una posición anterior.
            for (int i = 1; i < puntos; i++)
            {
                // Obtenemos la posición actual.
                posicionActual = new Vector2Int(ruidoX[i], ruidoY[i]);
                // Obtenemos la posición anerior.
                posicionAnterior = new Vector2Int(ruidoX[i - 1], ruidoY[i - 1]);

                // Calculamos la diferencia entre las dos.
                Vector2 diferencia = posicionActual - posicionAnterior;
                // Calculamos el cambio de altura.
                float cambioEnAltura = diferencia.y / intervalo;
                // Guardamos la altura acual 
                float alturaActual = posicionAnterior.y;

                // Generamos los bloques de dentro del intervalo desde la x anterior a la actual.
                for (int x = posicionAnterior.x; x < posicionActual.x && x <= mapa.GetUpperBound(0); x++)
                {
                    // Empezamos desde la altura actual
                    for (int y = Mathf.FloorToInt(alturaActual); y >= 0; y--)
                    {
                        mapa[x, y] = 1;
                    }
                    alturaActual += cambioEnAltura;
                }
            }
        }
        else
        {
            mapa = PerlinNoise(mapa, semilla);
        }
        return mapa;
    }

    /// <summary>
    /// Genera el terreno usando el algoritmo Random Walk
    /// </summary>
    /// <param name="mapa">Mapa a modificar.</param>
    /// <param name="semilla">La semilla que se utilizara pa alos números aleatorios.</param>
    /// <returns>El mapa modificado</returns>
    public static int[,] RandomWalk(int[,] mapa, float semilla)
    {
        // La semilla de nuestro Random.
        Random.InitState(semilla.GetHashCode());

        // Establecemos la altura inicial con la que empezaremos
        int ultimaAltura = Random.Range(0, mapa.GetUpperBound(1));

        // Recorremos todo el mapa a lo ancho.
        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            // 0 Sube, 1 baja, 2 igual
            int siguienteMovimiento = Random.Range(0, 3);

            // Subimos.
            if (siguienteMovimiento == 0 && ultimaAltura < mapa.GetUpperBound(1))
            {
                ultimaAltura++;
            }
            else if (siguienteMovimiento == 1 && ultimaAltura > 0)
            {
                ultimaAltura--;
            }
            // No cambiamos la altura.

            // Rellenamos de suelo desde la ultimaAltura hasta abajo
            for (int y = ultimaAltura; y >= 0; y--)
            {
                mapa[x, y] = 1;
            }
        }
        return mapa;
    }

    /// <summary>
    /// Genera el terreno usando el Random Walk suavizado.
    /// </summary>
    /// <param name="mapa">El mapa a modificar.</param>
    /// <param name="semilla">La semilla de los números aleatorios.</param>
    /// <param name="minimoAnchoSeccion">La mínima anchura de la seccion actual antes de cambiar la altura.</param>
    /// <returns></returns>
    public static int[,] RandomWalkSuavisado(int[,] mapa, float semilla, int minimoAnchoSeccion)
    {
        // La semilla de nuestro Random.
        Random.InitState(semilla.GetHashCode());

        // Establecemos la altura inicial con la que empezaremos
        int ultimaAltura = Random.Range(0, mapa.GetUpperBound(1));
        // Para llevar la cuenta del ancho de la seccion actual.
        int anchoSeccion = 0;

        // Recorremos todo el mapa a lo ancho.
        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            if (anchoSeccion >= minimoAnchoSeccion)
            {
                // 0 Sube, 1 baja, 2 igual.
                int siguienteMovimiento = Random.Range(0, 3);

                // Subimos.
                if (siguienteMovimiento == 0 && ultimaAltura < mapa.GetUpperBound(1))
                {
                    ultimaAltura++;
                }
                else if (siguienteMovimiento == 1 && ultimaAltura > 0)
                {
                    ultimaAltura--;
                }
                // No cambiamos la altura.

                // Empezamos una nueva seccion de seualo con la misma altura.
                anchoSeccion = 0;
            }

            // Hemos procesado otro bloque de la sección actual.
            anchoSeccion++;

            // Rellenamos de suelo desde la ultimaAltura hasta abajo.
            for (int y = ultimaAltura; y >= 0; y--)
            {
                mapa[x, y] = 1;
            }
        }
        return mapa;
    }

    /// <summary>
    /// Crea una cueva el Perlin Noise para el proceso de generación
    /// </summary>
    /// <param name="mapa">El mapa que se va a modificar.</param>
    /// <param name="modificador">El valor por el cual multiplicamos la posición para obtener un valor del Perlin Noise.</param>
    /// <param name="losBordesSonMuros">Si vale true, los bordes son muros. Si no, no.</param>
    /// <param name="offsetX">El desplazamiento en X para el Perlin Noise.</param>
    /// <param name="offsetY">El desplazamiento en Y para el Perlin Noise.</param>
    /// <param name="semilla">La semila que se usará para situarnos en un X,Y (X = Y = semilla) en el Prein Noise.</param>
    /// <returns>El mapa con la cueva generada con el Perlin Noise.</returns>
    public static int[,] PerlinNoiseCueva(int[,] mapa, float modificador, bool losBordesSonMuros, float offsetX = 0f, float offsetY = 0f, float semilla = 0f)
    {
        int nuevoPunto;
        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= mapa.GetUpperBound(1); y++)
            {
                if (losBordesSonMuros && (x == 0 || y == 0 || x == mapa.GetUpperBound(0) || y == mapa.GetUpperBound(1)))
                {
                    mapa[x, y] = 1;
                }
                else
                {
                    nuevoPunto = Mathf.RoundToInt(Mathf.PerlinNoise(x * modificador + offsetX + semilla, y * modificador + offsetY + semilla));
                    mapa[x, y] = nuevoPunto;
                }
            }
        }
        return mapa;
    }

    /// <summary>
    /// Crea una cueva usando el algoritmo Random Walk
    /// </summary>
    /// <param name="mapa">El mapa a modificar.</param>
    /// <param name="semilla">La semilla para los números aleatorios</param>
    /// <param name="porcentajeSueloAEliminar">La cantidad de suelo que queremos quitar.</param>
    /// <param name="losBordesSonMuros">Si hay que mantener los bordes siempre con suelo.</param>
    /// <Param name="movimientoEnDiagonal">Si se permite el movimiento en diagonal durante la generacion de los tuneles.</param>
    /// <returns>El mapa modificado</returns>
    public static int[,] RandomWalkCueva(int[,] mapa, float semilla, float porcentajeSueloAEliminar, bool losBordesSonMuros = true, bool movimientoEnDiagonal = false)
    {
        // La semilla de nuestro Random
        Random.InitState(semilla.GetHashCode());

        // Definimos los limites
        int valorMinimo = 0;
        int valorMaximoX = mapa.GetUpperBound(0);
        int valorMaximoY = mapa.GetUpperBound(1);
        int ancho = mapa.GetUpperBound(0) + 1;
        int alto = mapa.GetUpperBound(1) + 1;

        if (losBordesSonMuros)
        {
            valorMinimo++;
            valorMaximoX--;
            valorMaximoY--;
            ancho -= 2;
            alto -= 2;
        }

        // Definir la posición de inicio en X y en Y.
        int posicionX = Random.Range(valorMinimo, valorMaximoX);
        int posicionY = Random.Range(valorMinimo, valorMaximoY);

        // Calculamos la cantidad de losetas a eliminar.
        int cantidadDeLosetasAEliminar = Mathf.FloorToInt(ancho * alto * porcentajeSueloAEliminar);

        // Para contar cuantas losetas llevamos eliminadas.
        int losetasEliminadas = 0;

        while (losetasEliminadas < cantidadDeLosetasAEliminar)
        {
            if (mapa[posicionX, posicionY] == 1)
            {
                mapa[posicionX, posicionY] = 0;
                losetasEliminadas++;
            }

            if (movimientoEnDiagonal)
            {
                // Si nos podemos mover en diagonal
                int direccionAleatoriaX = Random.Range(-1, 2);
                int direccionAleatoriaY = Random.Range(-1, 2);
                posicionX += direccionAleatoriaX;
                posicionY += direccionAleatoriaY;
            }
            else
            {
                // Si no nos movemos en diagonal. Solo Arriba, Abajo, Izquierda y Derecha.
                int direccionAleatoria = Random.Range(0, 4);
                switch (direccionAleatoria)
                {
                    case 0:
                        // Arriba
                        posicionY++;
                        break;
                    case 1:
                        // Abajo
                        posicionY--;
                        break;
                    case 2:
                        // Izquierda
                        posicionX--;
                        break;
                    case 3:
                        // Derecha
                        posicionX++;
                        break;
                }
            }


            // Nos aseguramos de que no nos salimos del area de trabajo
            posicionX = Mathf.Clamp(posicionX, valorMinimo, valorMaximoX);
            posicionY = Mathf.Clamp(posicionY, valorMinimo, valorMaximoY);
        }
        return mapa;
    }

    /// <summary>
    /// Crea un tpunel de longitud en alto. Toma en cuenta la aspereza para cambiar el ancho del mismo.
    /// </summary>
    /// <param name="mapa">EL array que se modificará con el mapa.</param>
    /// <param name="semilla">La semilla para los números aleatorios</param>
    /// <param name="anchoMinimo">El ancho minimo del túnel.</param>
    /// <param name="anchoMaximo">El ancho máximo del túnel.</param>
    /// <Param name="aspereza">La probabilidad de que cambie de ancho en cada paso en el Y</param>
    /// <param name="desplazamientoMaximo">La cantidad máxima que podemos cambiar el punto central del túnel</param>
    /// <param name="desplazamiento">La probabilidad de que cambie el punto central del túnel</param>
    /// <returns>El mapa con el tunel direccional generado.</returns>
    public static int[,] TunelDireccional(int[,] mapa, float semilla, int anchoMinimo, int anchoMaximo, float aspereza, int desplazamientoMaximo, float desplazamiento)
    {
        // Este valor va desde su valor en negativo hasta el valor positivo,
        // en este caso, con el valor 1, el ancho del tunel es 3 (-1, 0, 1)
        int anchoTunel = 1;

        // La posicion X del centro del tunel
        int x = mapa.GetUpperBound(0) / 2;

        // La semilla de nuestro random
        Random.InitState(semilla.GetHashCode());

        // Recorremos en Y (queremos hacer el túnel vertical)
        for (int y = 0; y <= mapa.GetUpperBound(1); y++)
        {
            // Generamos esta parte del túnel.
            for (int i = -anchoTunel; i <= anchoTunel; i++)
            {
                mapa[x + i, y] = 0;
            }

            // Comprobamos si cambiamos el ancho basándonos en la aspereza
            if (Random.value < aspereza)
            {
                // Obtenemos aleatoriamente la cantidad que cambiaremos el ancho
                int cambioEnAncho = Random.Range(-anchoMaximo, anchoMaximo);
                anchoTunel += cambioEnAncho;

                // Nos aseguramos de que el tunel no se salga del mapa.
                anchoTunel = Mathf.Clamp(anchoTunel, anchoMinimo, anchoMaximo);
            }

            // Comprobamos si cambiamos la posición central del túnel
            if (Random.value < desplazamiento)
            {
                // Elegimos aleatoriamente el desplazamiento del túnel.
                int cambioEnX = Random.Range(-desplazamientoMaximo, desplazamientoMaximo);
                x += cambioEnX;

                // Nos aseguramos de que el túnel no se salga del mapa.
                x = Mathf.Clamp(x, anchoMaximo + 1, mapa.GetUpperBound(0) - anchoMaximo);
            }
        }
        return mapa;
    }

    /// <summary>
    /// Crea la base para las funciones avanzadas de automatas celulares
    /// Usaremos este mapa en distintas funciones dependiendo del tipo de vecindario que queremos
    /// </summary>
    /// <param name="ancho">Ancho del mapa.</param>
    /// <param name="alto">Alto del mapa.</param>
    /// <param name="semilla">La semilla de los números aleatorios.</param>
    /// <param name="porcentajeDeRelleno">La cantidad que queremos que se llene el mapa. Valor entre [0,1].</param>
    /// <param name="losBordesSonMuros">Si los bordes deben mantenerse.</param>   
    /// <returns>El con el contenido aleatorio</returns>
    public static int[,] GenerarMapaAleatorio(int ancho, int alto, float semilla, float porcentajeDeRelleno, bool losBordesSonMuros)
    {
        // La semilla de nuestro random.
        Random.InitState(semilla.GetHashCode());

        // Creamos el array.
        int[,] mapa = new int[ancho, alto];

        // Recorremos todas las posiciones del mapa.
        for (int x = 0; x <= mapa.GetUpperBound(0); x++)
        {
            for (int y = 0; y <= mapa.GetUpperBound(1); y++)
            {
                if(losBordesSonMuros && (x == 0 || x == mapa.GetUpperBound(0) || y == 0 || y == mapa.GetUpperBound(1))){
                    // Ponemos suelo si estamos en una posición del borde.
                    mapa[x, y] = 1;
                }
                else
                {
                 // Ponemos suelo si el resultado del random es inferior que el porcentaje de relleno.
                mapa[x, y] = (Random.value < porcentajeDeRelleno) ? 1 : 0;                   
                }
            }
        }
        return mapa;
    }

    /// <summary>
    /// Calcula el total de losetas vecinas
    /// </summary>
    /// <param name="mapa">Al mapa donde comprobar las losetas.</param>
    /// <param name="x">La posición en X de la loseta que estamos comprobando.</param>
    /// <param name="y">La posición en Y de la loseta que estamos comprobando.</param>
    /// <param name="incluirDiagonales">Si hay que tener en cuenta las posiciones vecinas en diagonal.</param>
    /// <returns>El total de losetas vecinas con suelo.</returns>
    public static int CantidadLosetasVecinas(int[,] mapa, int x, int y, bool incluirDiagonales)
    {
        // Lleva la cuenta de losteas vecinas
        int totalLosetas = 0;

        // Recorrer todas las posiciones vecinas.
        for(int vecinoX = x-1; vecinoX <= x+1; vecinoX++)
        {
            for(int vecinoY = y-1; vecinoY <= y+1; vecinoY++)
            {
                // Comprobamos que estamos dentro del mapa.
                if(vecinoX >= 0 && vecinoX <= mapa.GetUpperBound(0) && vecinoY >= 0 && vecinoY <= mapa.GetUpperBound(1))
                {
                    // Comprobamos que no estemos en la misma posición x, y que estamos comprobando.
                    // y si incluirDiagonales = false:
                    //
                    //   N
                    // N T N
                    //   N
                    //
                    // Si incluirDiagonales = true:
                    //
                    // N N N
                    // N T N
                    // N N N
                    //
                    if((vecinoX != x || vecinoY != y) && ( incluirDiagonales || (vecinoX == x || vecinoY == y)))
                    {
                        totalLosetas += mapa[vecinoX, vecinoY];
                    }
                }
            }
        }
        return totalLosetas;
    }

    /// <summary>
    /// Suaviza un mapa usando las reglas de vecindario de Moore.
    /// Se tienen en cuenta todas las losetas vecinas (incluidas las diagonales)
    /// </summary>
    /// <param name="mapa">El mapa a suavizar.</param>
    /// <param name="totalDePasadas">La cantidad de pasadas que haremos.</param>
    /// <param name="losBordesSonMuros">Si se deben mantener los bordes.</param>
    /// <returns>El mapa modificado.</returns>
    public static int[,] AutomataCelularMoore(int[,] mapa, int totalDePasadas, bool losBordesSonMuros)
    {
        for (int i = 0; i < totalDePasadas; i++)
        {
            for (int x = 0; x <= mapa.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= mapa.GetUpperBound(1); y++)
                {
                    // Obtenemos el total de losetas vecinas (Incluyendo las diagonales)
                    int losetasVecinas = CantidadLosetasVecinas(mapa, x, y, true);

                    // Si estamos en un borde y losBordesSonMuros está activado,
                    // ponemos un muro
                    if(losBordesSonMuros && (x == 0 || x == mapa.GetUpperBound(0) || y == 0 || y == mapa.GetUpperBound(1)))
                    {
                        mapa[x, y] = 1;
                    }

                    // Si tenemos mas de 4 vecinos, ponemos suelo.
                    else if(losetasVecinas > 4)
                    {
                        mapa[x, y] = 1;
                    }
                    // Si tenemos menos de 4 vecinos, dejamos un hueco.
                    else if(losetasVecinas < 4)
                    {
                        mapa[x, y] = 0;
                    }
                    // Si tenemos exactamente 4 vecinos, no cambiamos nada.
                }
            }
        }
        return mapa;
    }

    /// <summary>
    /// Suaviza un mapa usando las reglas de vecindario de Von Neumann.
    /// No se tienen en cuenta las losetas vecinas en diagonal
    /// </summary>
    /// <param name="mapa">El mapa a suavizar.</param>
    /// <param name="totalDePasadas">La cantidad de pasadas que haremos.</param>
    /// <param name="losBordesSonMuros">Si se deben mantener los bordes.</param>
    /// <returns>El mapa modificado.</returns>
    public static int[,] AutomataCelularVonNeumann(int[,] mapa, int totalDePasadas, bool losBordesSonMuros)
    {
        for (int i = 0; i < totalDePasadas; i++)
        {
            for (int x = 0; x <= mapa.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= mapa.GetUpperBound(1); y++)
                {
                    // Obtenemos el total de losetas vecinas (Incluyendo las diagonales)
                    int losetasVecinas = CantidadLosetasVecinas(mapa, x, y, false);

                    // Si estamos en un borde y losBordesSonMuros está activado,
                    // ponemos un muro
                    if(losBordesSonMuros && (x == 0 || x == mapa.GetUpperBound(0) || y == 0 || y == mapa.GetUpperBound(1)))
                    {
                        mapa[x, y] = 1;
                    }

                    // Si tenemos mas de 2 vecinos, ponemos suelo.
                    else if(losetasVecinas > 2)
                    {
                        mapa[x, y] = 1;
                    }
                    // Si tenemos menos de 2 vecinos, dejamos un hueco.
                    else if(losetasVecinas < 2)
                    {
                        mapa[x, y] = 0;
                    }
                    // Si tenemos exactamente 2 vecinos, no cambiamos nada.
                }
            }
        }
        return mapa;
    }
}
