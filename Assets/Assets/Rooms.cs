using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rooms 
{
   public RectangleGeneration positionGeneration;
   public int uniqueIndex;
   public int typeRoom;
   public Color color;
   public List<Rooms> voisin = new List<Rooms>();



    public float DistanceBetween2Room(Rooms room1, Rooms room2)
    {
     
      Vector2 distanceVec = room2.positionGeneration.originCenter- room1.positionGeneration.originCenter ;
      return distanceVec.magnitude;

    }

    public Vector2 VectorBetween2Room(Rooms room1, Rooms room2)
    {

        Vector2 distanceVec = room2.positionGeneration.originCenter - room1.positionGeneration.originCenter;
        return distanceVec;
    }

    public Vector2 DirectionBetween2Room(Rooms room1, Rooms room2)
    {
        Vector2 direction = (room2.positionGeneration.originCenter - room1.positionGeneration.originCenter);
        direction.Normalize();
        return direction;

    }

 


}
public class RectangleGeneration
{
    public Vector2 originLeftBottom;
    public Vector2 originCenter;
    public int height;
    public int width;
}







