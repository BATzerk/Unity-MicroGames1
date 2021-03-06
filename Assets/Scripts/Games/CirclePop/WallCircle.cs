﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CirclePop {
    public class WallCircle : Wall {
        // Components
        [SerializeField] private CircleCollider2D myCollider=null;


		// ----------------------------------------------------------------
		//  Doers
		// ----------------------------------------------------------------
		override public Prop SetSize(Vector2 _size) {
			base.SetSize(_size);
			myCollider.radius = _size.x*0.5f;
			return this;
		}

    }
}