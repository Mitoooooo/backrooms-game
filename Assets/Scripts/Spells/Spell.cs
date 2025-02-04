using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Spells
{
    public abstract class Spell : MonoBehaviour
    {
        public abstract void Cast();
    }
}
