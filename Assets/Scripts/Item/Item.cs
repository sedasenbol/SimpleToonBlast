using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
   [SerializeField] private ItemColor itemColor;
   [SerializeField] private Sprite[] sprites;
   [SerializeField] private SpriteRenderer spriteRenderer;

   private ItemCondition condition;
   private Transform myTransform;
   
   private void OnEnable()
   {
      myTransform = transform;
   }

   public void ChangeMyCondition(ItemCondition itemCondition)
   {
      condition = itemCondition;
      ChangeMySprite();
   }

   private void ChangeMySprite()
   {
      spriteRenderer.sprite = sprites[(int)condition];
   }

   private void OnDisable()
   {
      myTransform = null;
   }

   public Transform MyTransform => myTransform;
   public ItemColor Color => itemColor;
   public int IndexI { get; set; }
   public int IndexJ { get; set; }
}
