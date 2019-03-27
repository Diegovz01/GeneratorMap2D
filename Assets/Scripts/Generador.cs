using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Algoritmo
{
    PerlinNoise, PerlinNoiseSuavizado, RandomWalk, RandomWalkSueavizado,
    PerlinNoiseCueva, RandomWalkCueva, TunelDireccional, MapaAleatorio,
    AutomataCelularMoore, AutomataCelularVonNeumann
}
public class Generador : MonoBehaviour
{
    /* Ejemplo
    public bool propiedadDelObjeto;
    private void Start()
    {
        GenerarMapa();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G) || Input.GetMouseButtonDown(0))
        {
            GenerarMapa();
        }
        if (Input.GetKeyDown(KeyCode.L) || Input.GetMouseButtonDown(1))
        {
            LimpiarMapa();
        }
        
    }*/
    [Header("Referencias")]
    public Tilemap MapaDeLosetas;
    public TileBase Loseta;

    [Header("Dimensiones")]
    public int Ancho = 60;
    public int Alto = 34;

    [Header("Semillas")]
    public bool SemillaAleatoria = true;
    public float Semilla = 0;
    
    [Header("Algoritmo")]
    public Algoritmo algoritmo = Algoritmo.PerlinNoise;

    [Header("Perlin Noise Suavizado")]
    public int Intervalo = 2;

    [Header("Random Walk Suavisado")]
    public int MinimoAnchoSeccion = 2;

    [Header("Cuevas")]
    public bool LosBordesSonMuros = true;

    [Header("Perlin Noise Cueva")]
    public float Modificador = 0.1f;
    public float OffsetX = 0f;
    public float OffsetY = 0f;

    [Header("Random Walk Cueva")]
    [Range(0,1)]
    public float PorcentajeAEliminar = 0.25f;
    public bool MovimientoEnDiagonal = false;

    [Header("Túnel Direccional")]
    public int AnchoMaximo = 4;
    public int AnchoMinimo = 1;
    public int DesplazamientoMaximo = 2;
    [Range(0, 1)]
    public float Aspereza = 0.75f;
    [Range(0, 1)]
    public float Desplazamiento = 0.75f;

    [Header("Autómata celular")]
    [Range(0, 1)]
    public float PorcentajeDeRelleno = 0.45f;
    public int TotalDePasadas = 3;

    public void GenerarMapa()
    {
        /*
        int[,] temporal = new int[,] { {1, 1, 0},
                                        {1, 1, 1},
                                        {1, 0, 1} };

        Debug.Log(Metodos.CantidadLosetasVecinas(temporal, 1, 1, true));
        */    

        // Limpiamos el mapa de losetas.
        MapaDeLosetas.ClearAllTiles();

        // Creamos el array bidimensional del mapa
        int[,] mapa = null;

        // Generamos una semilla nueva de forma aleatoria.
        if (SemillaAleatoria)
        {
            Semilla = Random.Range(0f, 1000f);
        }

        switch (algoritmo)
        {
            case Algoritmo.PerlinNoise:
                mapa = Metodos.GenerarArray(Ancho, Alto, true);
                mapa = Metodos.PerlinNoise(mapa, Semilla);
                break;

            case Algoritmo.PerlinNoiseSuavizado:
                mapa = Metodos.GenerarArray(Ancho,Alto, true);
                mapa = Metodos.PerlinNoiseSuavisado( mapa, Semilla, Intervalo);
                break;
            
            case Algoritmo.RandomWalk:
                mapa = Metodos.GenerarArray(Ancho, Alto, true);
                mapa = Metodos.RandomWalk(mapa, Semilla);
                break;
            
            case Algoritmo.RandomWalkSueavizado:
                mapa = Metodos.GenerarArray(Ancho, Alto, true);
                mapa = Metodos.RandomWalkSuavisado(mapa, Semilla, MinimoAnchoSeccion);
                break;
            
            case Algoritmo.PerlinNoiseCueva:
                mapa = Metodos.GenerarArray(Ancho, Alto, false);
                mapa = Metodos.PerlinNoiseCueva(mapa, Modificador, LosBordesSonMuros, OffsetX, OffsetY, Semilla);
                break;
            
            case Algoritmo.RandomWalkCueva:
                mapa = Metodos.GenerarArray(Ancho, Alto, false);
                mapa = Metodos.RandomWalkCueva(mapa, Semilla, PorcentajeAEliminar, LosBordesSonMuros, MovimientoEnDiagonal);
                break;

            case Algoritmo.TunelDireccional:
                mapa = Metodos.GenerarArray(Ancho, Alto, false);
                mapa = Metodos.TunelDireccional(mapa, Semilla, AnchoMinimo, AnchoMaximo, Aspereza, DesplazamientoMaximo, Desplazamiento);
                break;
            case Algoritmo.MapaAleatorio:
                mapa = Metodos.GenerarMapaAleatorio(Ancho, Alto, Semilla, PorcentajeDeRelleno, LosBordesSonMuros);
                break;
            case Algoritmo.AutomataCelularMoore:
                mapa = Metodos.GenerarMapaAleatorio(Ancho, Alto, Semilla, PorcentajeDeRelleno, LosBordesSonMuros);
                mapa = Metodos.AutomataCelularMoore(mapa, TotalDePasadas, LosBordesSonMuros);
                break;
            case Algoritmo.AutomataCelularVonNeumann:
                mapa = Metodos.GenerarMapaAleatorio(Ancho, Alto, Semilla, PorcentajeDeRelleno, LosBordesSonMuros);
                mapa = Metodos.AutomataCelularVonNeumann(mapa, TotalDePasadas, LosBordesSonMuros);
                break;
        }

        //= Metodos.GenerarArray(Ancho, Alto, false);
        Metodos.GenerarMapa(mapa, MapaDeLosetas, Loseta);
        Debug.Log("Generar Mapa");
    }
    public void LimpiarMapa()
    {
        MapaDeLosetas.ClearAllTiles();
        Debug.Log("Limpiar Mapa");
    }
}
