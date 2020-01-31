using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationsRooms : MonoBehaviour
{

    enum TypeRooms
    {
        Nothing,
        Salle,
        Couloir,
        Mur,
        Tresor,
        Trap


    }


    // Start is called before the first frame update
    [SerializeField] public int numberRooms;
    [SerializeField] public Vector2Int sizePlateau;
    [SerializeField] public int minWidth;
    [SerializeField] public int maxWidth;
    [SerializeField] public int minHeight;
    [SerializeField] public int maxHeight;

    [SerializeField] public int limitGenerationCouloir = 5;
    [SerializeField] public int verifCouloirGeneration = 1;
    [SerializeField] public int pourcentTrap = 10;


    private int numberCaseRooms = 0;

    string ligne;
    public Rooms[] tabRooms;
    public int[,] generationPlateau;
    [SerializeField] public int numberCaseExcluVoisin;
    private bool isLCouloir = false;

    List<int> voisinAlreadyGenerate = new List<int>();
    List<Vector2Int> redRoomsBlock = new List<Vector2Int>();

    bool couloirAlreadyGenerate = false;




    [SerializeField] public GameObject[] prefabsGeneration;
    void Start()
    {
        generationPlateau = new int[sizePlateau.x, sizePlateau.y];
        for (int j = 0; j < sizePlateau.x; j++)
        {
            for (int k = 0; k < sizePlateau.y; k++)
            {
                generationPlateau[j, k] = (int)TypeRooms.Nothing;
            }

        }
        tabRooms = new Rooms[numberRooms];

        CreateRooms();


        bool isOk = true;
        for (int i = 0; i < tabRooms.Length; i++)
        {
            int startOriginsX = 0;
            int startOriginsY = 0;
            int widthCaseX = 0;
            int widthCaseY = 0;
            do
            {
                isOk = true;
                do
                {
                    startOriginsX = Random.Range(0, sizePlateau.x);
                } while (startOriginsX > sizePlateau.x - maxWidth);

                do
                {
                    startOriginsY = Random.Range(0, sizePlateau.y);

                }
                while (startOriginsY > sizePlateau.y - maxHeight);

                widthCaseX = startOriginsX + tabRooms[i].positionGeneration.width;
                widthCaseY = startOriginsY + tabRooms[i].positionGeneration.height;
                for (int j = startOriginsX; j < widthCaseX; j++)
                {
                    for (int k = startOriginsY; k < widthCaseY; k++)
                    {
                        if (generationPlateau[j, k] != 0)
                        {
                            isOk = false;

                        }
                    }

                }

            } while (isOk == false);




            for (int j = startOriginsX; j < widthCaseX; j++)
            {
                for (int k = startOriginsY; k < widthCaseY; k++)
                {

                    generationPlateau[j, k] = tabRooms[i].typeRoom;
                }
            }
            tabRooms[i].positionGeneration.originCenter = new Vector2(startOriginsX + tabRooms[i].positionGeneration.width / 2, startOriginsY + tabRooms[i].positionGeneration.height / 2);
            tabRooms[i].positionGeneration.originLeftBottom = new Vector2(startOriginsX, startOriginsY);
        }




        GenerateVoisin();
        GenerateCouloirs();
        GenerateWall();
        GenerateTrap();
        InstantiateDungeon();



        for (int i = 0; i < tabRooms.Length; i++)
        {
            for (int j = 0; j < tabRooms[i].voisin.Count; j++)
            {
                ligne += tabRooms[i].voisin[j].uniqueIndex.ToString() + " , ";

            }

            Debug.Log("Room : " + tabRooms[i].uniqueIndex + "Voisin : " + ligne);
            ligne = "";
            Debug.Log("Room : " + tabRooms[i].uniqueIndex + "Centre : " + tabRooms[i].positionGeneration.originCenter);
        }



    }

    // Update is called once per frame
    void Update()
    {

    }


    public Rooms GetRoom(int index)
    {
        for (int i = 0; i < tabRooms.Length; i++)
        {
            if (tabRooms[i].uniqueIndex == index)
            {
                return tabRooms[i];
            }
        }

        return null;
    }

    void CreateRooms()
    {
        for (int i = 0; i < numberRooms; i++)
        {
            Rooms tempRoom = new Rooms();
            tempRoom.positionGeneration = new RectangleGeneration();
            tempRoom.color = Color.red;
            tempRoom.uniqueIndex = i;
            tempRoom.typeRoom = (int)TypeRooms.Salle;

            tempRoom.positionGeneration.width = Random.Range(minWidth, maxWidth);
            tempRoom.positionGeneration.height = Random.Range(minHeight, maxHeight);

            tabRooms[i] = tempRoom;
        }
    }

    void GenerateVoisin()
    {
        for (int i = 0; i < tabRooms.Length; i++)
        {
            float distanceMin = 1000000;
            for (int j = 0; j < tabRooms.Length; j++)
            {
                if (tabRooms[i].uniqueIndex != tabRooms[j].uniqueIndex)
                {
                    float distance = tabRooms[i].DistanceBetween2Room(tabRooms[i], tabRooms[j]);
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                    }
                }
            }

            for (int j = 0; j < tabRooms.Length; j++)
            {
                if (tabRooms[i].uniqueIndex != tabRooms[j].uniqueIndex)
                {

                    float distance = tabRooms[i].DistanceBetween2Room(tabRooms[i], tabRooms[j]);
                    if (distance < distanceMin + limitGenerationCouloir)
                    {
                        tabRooms[i].voisin.Add(GetRoom(j));
                    }


                }

            }
        }


    }


    void GenerateWall()
    {
        for (int j = 0; j < sizePlateau.x; j++)
        {
            for (int k = 0; k < sizePlateau.y; k++)
            {
                if (generationPlateau[j, k] == (int)TypeRooms.Salle)
                {

                    numberCaseRooms++;
                    redRoomsBlock.Add(new Vector2Int(j, k));
                }

                if (generationPlateau[j, k] == (int)TypeRooms.Nothing)
                {

                    if (CheckLimitsMur(new Vector2(j + 1, k)))
                        if (generationPlateau[j + 1, k] == (int)TypeRooms.Salle || generationPlateau[j + 1, k] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;


                    if (CheckLimitsMur(new Vector2(j + 1, k - 1)))
                        if (generationPlateau[j + 1, k - 1] == (int)TypeRooms.Salle || generationPlateau[j + 1, k - 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j, k - 1)))
                        if (generationPlateau[j, k - 1] == (int)TypeRooms.Salle || generationPlateau[j, k - 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j - 1, k - 1)))
                        if (generationPlateau[j - 1, k - 1] == (int)TypeRooms.Salle || generationPlateau[j - 1, k - 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j - 1, k)))
                        if (generationPlateau[j - 1, k] == (int)TypeRooms.Salle || generationPlateau[j - 1, k] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j - 1, k + 1)))
                        if (generationPlateau[j - 1, k + 1] == (int)TypeRooms.Salle || generationPlateau[j - 1, k + 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j, k + 1)))
                        if (generationPlateau[j, k + 1] == (int)TypeRooms.Salle || generationPlateau[j, k + 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;

                    if (CheckLimitsMur(new Vector2(j + 1, k + 1)))
                        if (generationPlateau[j + 1, k + 1] == (int)TypeRooms.Salle || generationPlateau[j + 1, k + 1] == (int)TypeRooms.Couloir)
                            generationPlateau[j, k] = (int)TypeRooms.Mur;




                }
            }
        }
    }


    void GenerateTrap()
    {
        int numberTrap = numberCaseRooms * pourcentTrap / 100;
        for (int i = 0; i < numberTrap; i++)
        {
            int j = 0;
            bool isGoodTrap = true;
            do
            {
                 isGoodTrap = true;
                j = Random.Range(0, redRoomsBlock.Count);

                Vector2Int pos = new Vector2Int(redRoomsBlock[j].x, redRoomsBlock[j].y);
                if (generationPlateau[pos.x, pos.y] == (int)TypeRooms.Salle)
                {
                  
                    if (CheckLimitsMur(new Vector2(pos.x + 1, pos.y)))
                        if (generationPlateau[pos.x+1, pos.y] != (int)TypeRooms.Salle)
                            isGoodTrap = false;


                    if (CheckLimitsMur(new Vector2(pos.x + 1, pos.y - 1)))
                        if (generationPlateau[pos.x+1, pos.y-1] != (int)TypeRooms.Salle)
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x, pos.y - 1)))
                        if (generationPlateau[pos.x, pos.y-1] != (int)TypeRooms.Salle)
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x - 1, pos.y - 1)))
                        if (generationPlateau[pos.x-1, pos.y-1] != (int)TypeRooms.Salle )
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x - 1, pos.y)))
                        if (generationPlateau[pos.x-1, pos.y] != (int)TypeRooms.Salle )
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x - 1, pos.y + 1)))
                        if (generationPlateau[pos.x-1, pos.y+1] != (int)TypeRooms.Salle)
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x, pos.y + 1)))
                        if (generationPlateau[pos.x, pos.y + 1] != (int)TypeRooms.Salle )
                            isGoodTrap = false;

                    if (CheckLimitsMur(new Vector2(pos.x + 1, pos.y + 1)))
                        if (generationPlateau[pos.x+1, pos.y + 1] != (int)TypeRooms.Salle)
                            isGoodTrap = false;


                    if (isGoodTrap)
                    {
                        generationPlateau[pos.x, pos.y] = (int)TypeRooms.Trap;
                    }
                }
                else
                {
                    isGoodTrap = false;
                }
            } while (!isGoodTrap);
            
        }
    }

    void GenerateTresor()
    {
      
    }
    void SetOrientationWall()
    {

    }
    void SetEntryEnd()
    {

    }


    void GenerateCouloirs()
    {
        #region Tentative1
        /* List<int> tempDejafait = new List<int>();
         bool alreadyDone = false;
         Vector2 caseTest = new Vector2();
         for (int i = 0; i < tabRooms.Length; i++)
         {
             for (int j = 0; j < tabRooms[i].voisin.Count; j++)
             {

                 for(int k = 0; k < tempDejafait.Count; k++)
                 {
                     if(tempDejafait[k] == tabRooms[i].voisin[j].uniqueIndex)
                     {
                         //alreadyDone = true;
                     }
                 }
                 if (!alreadyDone)
                 {
                     Vector2 vectorCouloir = tabRooms[i].VectorBetween2Room(tabRooms[i], tabRooms[i].voisin[j]);
                     caseTest = tabRooms[i].positionGeneration.originCenter;
                     Vector2 direction = tabRooms[i].DirectionBetween2Room(tabRooms[i], tabRooms[i].voisin[j]);


                     bool isOk = false;
                     do
                     {
                         caseTest.x +=  direction.x;
                         caseTest.y +=  direction.y;
                         if ((int)caseTest.x < sizePlateau.x && (int)caseTest.y < sizePlateau.y && (int)caseTest.x > 0 && (int)caseTest.y > 0)
                         {
                             if (generationPlateau[(int)caseTest.x, (int)caseTest.y] == (int)TypeRooms.Nothing)
                             {

                                generationPlateau[(int)caseTest.x, (int)caseTest.y] = (int)TypeRooms.Couloir;

                                 if ((int)caseTest.x + 1 < sizePlateau.x && (int)caseTest.y < sizePlateau.y && (int)caseTest.x + 1 > 0 && (int)caseTest.y > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x + 1, (int)caseTest.y] == (int)TypeRooms.Nothing)
                                     {
                                         generationPlateau[(int)caseTest.x + 1, (int)caseTest.y] = (int)TypeRooms.Couloir;
                                     }
                                 }
                                 if ((int)caseTest.x - 1 < sizePlateau.x && (int)caseTest.y < sizePlateau.y && (int)caseTest.x - 1 > 0 && (int)caseTest.y > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x - 1, (int)caseTest.y] == (int)TypeRooms.Nothing)
                                     {
                                         generationPlateau[(int)caseTest.x - 1, (int)caseTest.y] = (int)TypeRooms.Couloir;
                                     }

                                 }
                                 if ((int)caseTest.x < sizePlateau.x && (int)caseTest.y + 1 < sizePlateau.y && (int)caseTest.x > 0 && (int)caseTest.y + 1 > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x, (int)caseTest.y + 1] == (int)TypeRooms.Nothing)
                                     {
                                        generationPlateau[(int)caseTest.x, (int)caseTest.y + 1] = (int)TypeRooms.Couloir;

                                     }
                                 }
                                 if ((int)caseTest.x < sizePlateau.x && (int)caseTest.y - 1 < sizePlateau.y && (int)caseTest.x > 0 && (int)caseTest.y - 1 > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x, (int)caseTest.y - 1] == (int)TypeRooms.Nothing)
                                     {
                                        generationPlateau[(int)caseTest.x, (int)caseTest.y - 1] = (int)TypeRooms.Couloir;
                                     }
                                 }
                                 if ((int)caseTest.x + 1 < sizePlateau.x && (int)caseTest.y+1 < sizePlateau.y && (int)caseTest.x + 1 > 0 && (int)caseTest.y+1 > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x + 1, (int)caseTest.y+1] == (int)TypeRooms.Nothing)
                                     {
                                         generationPlateau[(int)caseTest.x + 1, (int)caseTest.y+1] = (int)TypeRooms.Couloir;
                                     }
                                 }
                                 if ((int)caseTest.x - 1 < sizePlateau.x && (int)caseTest.y - 1 < sizePlateau.y && (int)caseTest.x - 1 > 0 && (int)caseTest.y - 1 > 0)
                                 {
                                     if (generationPlateau[(int)caseTest.x - 1, (int)caseTest.y - 1] == (int)TypeRooms.Nothing)
                                     {
                                         generationPlateau[(int)caseTest.x - 1, (int)caseTest.y - 1] = (int)TypeRooms.Couloir;
                                     }
                                 }
                             }

                         }

                         if (direction.x >= 0)
                         {
                             if (direction.y >= 0)
                             {
                                 if (caseTest.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x && caseTest.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                 {
                                     isOk = true;
                                     Debug.Log("isOK1");
                                 }
                             }
                             else
                             {
                                 if (caseTest.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x && caseTest.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                 {
                                     isOk = true;
                                     Debug.Log("isOK2");
                                 }

                             }

                         }
                         else
                         {

                             if (direction.y >= 0)
                             {
                                 if (caseTest.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x && caseTest.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                 {
                                     isOk = true;
                                     Debug.Log("isOK3");
                                 }
                             }
                             else
                             {
                                 if (caseTest.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x && caseTest.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                 {
                                     isOk = true;
                                     Debug.Log("isOK4");

                                 }

                             }

                         }

                     } while (!isOk);
                 }
                 alreadyDone = false;
                // Debug.Log("Point/Droite : " + caseTest + "Voisin : " + tabRooms[i].voisin[j].positionGeneration.originCenter);
             }
             tempDejafait.Add(tabRooms[i].uniqueIndex);
         }*/
        #endregion

        for (int i = 0; i < tabRooms.Length; i++)
        {
            for (int j = 0; j < tabRooms[i].voisin.Count; j++)
            {
                #region TestAxeAligne


                for (int k = 0; k < voisinAlreadyGenerate.Count; k++)
                {

                    if (voisinAlreadyGenerate[k] == tabRooms[i].voisin[j].uniqueIndex)
                    {
                        couloirAlreadyGenerate = true;
                    }
                }
                if (!couloirAlreadyGenerate)
                {
                    Vector2 setCouloir = tabRooms[i].positionGeneration.originCenter;
                    Vector2 direction = tabRooms[i].DirectionBetween2Room(tabRooms[i], tabRooms[i].voisin[j]);

                    bool isOk = false;
                    bool isOkC = false;
                    // Generation Couloir 
                    do
                    {
                        //Meme X EN FACE
                        if (tabRooms[i].positionGeneration.originCenter.x == tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                        {

                            setCouloir.y += direction.y;
                            if (CheckLimits(setCouloir))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                            }
                            if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                            }
                            if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                            }

                            if (direction.y >= 0)
                            {
                                if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                    isOk = true;
                            }
                            else
                            {
                                if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                    isOk = true;
                            }


                        }
                        //meme Y EN FACE
                        else if (tabRooms[i].positionGeneration.originCenter.y == tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                        {


                            setCouloir.x += direction.x;

                            if (CheckLimits(setCouloir))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                            }
                            if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                            }
                            if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                            {
                                generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                            }

                            if (direction.x >= 0)
                            {
                                if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                    isOk = true;
                            }
                            else
                            {
                                if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                    isOk = true;
                            }


                        }

                        else
                        {
                            float indexY = 0;
                            float indexX = 0;
                            //   if (tabRooms[i].positionGeneration.originCenter.x)
                            //GESTION DES 4 CAS DE FIGURE HAUT GAUCHE

                            if (!isLCouloir)
                            {

                                if (direction.x > 0 & direction.y > 0)
                                {

                                    // DEUX CAS PAR CHAQUE CAS HAUT GAUCHE
                                    if (tabRooms[i].positionGeneration.originCenter.x + (int)(tabRooms[i].positionGeneration.width / 2) - numberCaseExcluVoisin >=
                                        tabRooms[i].voisin[j].positionGeneration.originCenter.x - (int)(tabRooms[i].voisin[j].positionGeneration.width / 2) + numberCaseExcluVoisin)
                                    {
                                        indexX = (int)(Mathf.Abs(tabRooms[i].positionGeneration.originCenter.x - tabRooms[i].voisin[j].positionGeneration.originCenter.x) / 2);
                                        setCouloir.x = tabRooms[i].positionGeneration.originCenter.x + indexX;
                                        setCouloir.y += 1;


                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else if (tabRooms[i].positionGeneration.originCenter.y + (int)(tabRooms[i].positionGeneration.height / 2) - numberCaseExcluVoisin >=
                                        tabRooms[i].voisin[j].positionGeneration.originCenter.y - (int)(tabRooms[i].voisin[j].positionGeneration.height / 2) + numberCaseExcluVoisin)
                                    {
                                        indexY = (int)(Mathf.Abs(tabRooms[i].voisin[j].positionGeneration.originCenter.y - tabRooms[i].positionGeneration.originCenter.y) / 2);
                                        setCouloir.x += 1;
                                        setCouloir.y = tabRooms[i].positionGeneration.originCenter.y + indexY;



                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else
                                    {
                                        isLCouloir = true;
                                    }

                                }

                                else if (direction.x < 0 & direction.y < 0)
                                {
                                    if (tabRooms[i].positionGeneration.originCenter.x - (int)(tabRooms[i].positionGeneration.width / 2) + numberCaseExcluVoisin <=
                                    tabRooms[i].voisin[j].positionGeneration.originCenter.x + (int)(tabRooms[i].voisin[j].positionGeneration.width / 2) - numberCaseExcluVoisin)
                                    {
                                        indexX = (int)(Mathf.Abs(tabRooms[i].positionGeneration.originCenter.x - tabRooms[i].voisin[j].positionGeneration.originCenter.x) / 2);
                                        setCouloir.x = tabRooms[i].positionGeneration.originCenter.x - indexX;
                                        setCouloir.y -= 1;


                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else if (tabRooms[i].positionGeneration.originCenter.y - (int)(tabRooms[i].positionGeneration.height / 2) + numberCaseExcluVoisin <=
                                        tabRooms[i].voisin[j].positionGeneration.originCenter.y + (int)(tabRooms[i].voisin[j].positionGeneration.height / 2) - numberCaseExcluVoisin)
                                    {
                                        indexY = (int)(Mathf.Abs(tabRooms[i].voisin[j].positionGeneration.originCenter.y - tabRooms[i].positionGeneration.originCenter.y) / 2);
                                        setCouloir.x -= 1;
                                        setCouloir.y = tabRooms[i].positionGeneration.originCenter.y - indexY;



                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                        {

                                            isOk = true;
                                        }
                                    }
                                    else
                                    {
                                        isLCouloir = true;

                                    }
                                }

                                else if (direction.x > 0 & direction.y < 0)
                                {

                                    if (tabRooms[i].positionGeneration.originCenter.x + (int)(tabRooms[i].positionGeneration.width / 2) - numberCaseExcluVoisin >=
                                       tabRooms[i].voisin[j].positionGeneration.originCenter.x - (int)(tabRooms[i].voisin[j].positionGeneration.width / 2) + numberCaseExcluVoisin)
                                    {
                                        indexX = (int)(Mathf.Abs(tabRooms[i].positionGeneration.originCenter.x - tabRooms[i].voisin[j].positionGeneration.originCenter.x) / 2);
                                        setCouloir.x = tabRooms[i].positionGeneration.originCenter.x + indexX;
                                        setCouloir.y -= 1;


                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                        {
                                            isOk = true;
                                        }
                                    }

                                    else if (tabRooms[i].positionGeneration.originCenter.y - (int)(tabRooms[i].positionGeneration.height / 2) + numberCaseExcluVoisin <=
                                       tabRooms[i].voisin[j].positionGeneration.originCenter.y + (int)(tabRooms[i].voisin[j].positionGeneration.height / 2) - numberCaseExcluVoisin)
                                    {
                                        indexY = (int)(Mathf.Abs(tabRooms[i].voisin[j].positionGeneration.originCenter.y - tabRooms[i].positionGeneration.originCenter.y) / 2);
                                        setCouloir.x += 1;
                                        setCouloir.y = tabRooms[i].positionGeneration.originCenter.y - indexY;



                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else
                                    {
                                        isLCouloir = true;
                                    }
                                }

                                else if (direction.x < 0 & direction.y > 0)
                                {

                                    if (tabRooms[i].positionGeneration.originCenter.x - tabRooms[i].positionGeneration.width / 2 + numberCaseExcluVoisin <=
                                      tabRooms[i].voisin[j].positionGeneration.originCenter.x + tabRooms[i].voisin[j].positionGeneration.width / 2 - numberCaseExcluVoisin)
                                    {
                                        indexX = (int)(Mathf.Abs(tabRooms[i].positionGeneration.originCenter.x - tabRooms[i].voisin[j].positionGeneration.originCenter.x) / 2);
                                        setCouloir.x = tabRooms[i].positionGeneration.originCenter.x - indexX;
                                        setCouloir.y += 1;


                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else if (tabRooms[i].positionGeneration.originCenter.y + tabRooms[i].positionGeneration.height / 2 - numberCaseExcluVoisin >=
                                       tabRooms[i].voisin[j].positionGeneration.originCenter.y - tabRooms[i].voisin[j].positionGeneration.height / 2 + numberCaseExcluVoisin)
                                    {
                                        indexY = (int)(Mathf.Abs(tabRooms[i].voisin[j].positionGeneration.originCenter.y - tabRooms[i].positionGeneration.originCenter.y) / 2);
                                        setCouloir.x -= 1;
                                        setCouloir.y = tabRooms[i].positionGeneration.originCenter.y + indexY;



                                        if (CheckLimits(setCouloir))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                        {
                                            generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                        }
                                        if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                        {
                                            isOk = true;
                                        }
                                    }
                                    else
                                    {
                                        isLCouloir = true;
                                    }
                                }
                            }
                            else if (isLCouloir)
                            {
                                //  LLLL

                                Debug.Log("direction :" + direction);

                                if (direction.x > 0 && direction.y > 0)
                                {
                                    if (direction.x >= direction.y)
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x += 1;
                                                setCouloir.y = tabRooms[i].positionGeneration.originCenter.y;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.y += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                    else
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x = tabRooms[i].positionGeneration.originCenter.x;
                                                setCouloir.y += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.x += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                }

                                if (direction.x < 0 && direction.y < 0)
                                {
                                    if (direction.x <= direction.y)
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x -= 1;
                                                setCouloir.y = tabRooms[i].positionGeneration.originCenter.y;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.y -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                    else
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x = tabRooms[i].positionGeneration.originCenter.x;
                                                setCouloir.y -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.x -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }

                                }

                                if (direction.x > 0 && direction.y < 0)
                                {
                                    if (direction.x >= Mathf.Abs(direction.y))
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x += 1;
                                                setCouloir.y = tabRooms[i].positionGeneration.originCenter.y;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.y -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                    else
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x = tabRooms[i].positionGeneration.originCenter.x;
                                                setCouloir.y -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y <= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.x += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x >= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;


                                    }

                                }


                                if (direction.x < 0 && direction.y > 0)
                                {
                                    if (Mathf.Abs(direction.x) > direction.y)
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x -= 1;
                                                setCouloir.y = tabRooms[i].positionGeneration.originCenter.y;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y - 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.y += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                    else
                                    {
                                        do
                                        {
                                            if (!isOkC)
                                            {
                                                setCouloir.x = tabRooms[i].positionGeneration.originCenter.x;
                                                setCouloir.y += 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x - 1, setCouloir.y)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x - 1), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.y >= tabRooms[i].voisin[j].positionGeneration.originCenter.y)
                                                {
                                                    if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }

                                                    if (CheckLimits(new Vector2(setCouloir.x + 1, setCouloir.y + 1)))
                                                    {
                                                        generationPlateau[Mathf.RoundToInt(setCouloir.x + 1), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                    }
                                                    isOkC = true;
                                                    isOk = false;
                                                }
                                            }
                                            else
                                            {
                                                setCouloir.x -= 1;
                                                if (CheckLimits(setCouloir))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y + 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y + 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (CheckLimits(new Vector2(setCouloir.x, setCouloir.y - 1)))
                                                {
                                                    generationPlateau[Mathf.RoundToInt(setCouloir.x), Mathf.RoundToInt(setCouloir.y - 1)] = (int)TypeRooms.Couloir;
                                                }
                                                if (setCouloir.x <= tabRooms[i].voisin[j].positionGeneration.originCenter.x)
                                                {
                                                    isOk = true;
                                                }

                                            }
                                        } while (!isOk);
                                        isOkC = false;
                                        isOk = true;
                                    }
                                    isOk = true;
                                }


                                isLCouloir = false;
                            }
                            else
                            {
                                Debug.Log(" Tu ne devrais pas rentrer la");
                                isOk = true;
                            }


                        }

                    } while (!isOk);

                }
                else
                {
                    couloirAlreadyGenerate = false;
                }

                #endregion
            }
            voisinAlreadyGenerate.Add(tabRooms[i].uniqueIndex);
            Debug.Log(tabRooms[i].uniqueIndex);
            couloirAlreadyGenerate = false;
        }
    }
    void InstantiateDungeon()
    {

        for (int j = 0; j < sizePlateau.x; j++)
        {
            for (int k = 0; k < sizePlateau.y; k++)
            {

                GameObject go = Instantiate(prefabsGeneration[0], new Vector3(j, k, 1), Quaternion.identity);
                if (generationPlateau[j, k] == (int)TypeRooms.Salle)
                {
                    go.GetComponent<MeshRenderer>().material.color = GetRoom(generationPlateau[j, k]).color;
                }
                else if (generationPlateau[j, k] == (int)TypeRooms.Couloir)
                {
                    go.GetComponent<MeshRenderer>().material.color = Color.blue;
                }
                else if (generationPlateau[j, k] == (int)TypeRooms.Mur)
                {
                    go.GetComponent<MeshRenderer>().material.color = Color.black;
                }
                else if (generationPlateau[j, k] == (int)TypeRooms.Trap)
                {
                    go.GetComponent<MeshRenderer>().material.color = Color.grey;
                }
                else
                {
                    go.GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }

        }


    }



    bool CheckLimits(Vector2 limit)
    {
        if (limit.x > 0 && limit.x < sizePlateau.x && limit.y > 0 && limit.y < sizePlateau.y)
        {
            if (generationPlateau[(int)limit.x, (int)limit.y] == (int)TypeRooms.Nothing)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }


    bool CheckLimitsMur(Vector2 limit)
    {
        if (limit.x > 0 && limit.x < sizePlateau.x && limit.y > 0 && limit.y < sizePlateau.y)
        {

            return true;
        }
        else
        {
            return false;
        }
    }

}
