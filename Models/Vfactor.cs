﻿using System;
using System.Collections.Generic;

namespace Reporteadores.Models;

public partial class Vfactor
{
    public short VlCodigo { get; set; }

    public short VfOrden { get; set; }

    public string VfCodigo { get; set; } = null!;

    public string VfNombre { get; set; } = null!;

    public string? VfDescrip { get; set; }

    public string VfIngles { get; set; } = null!;

    public string? VfDesIng { get; set; }

    public decimal VfNumero { get; set; }

    public string VfTexto { get; set; } = null!;

    public int Llave { get; set; }

    public virtual Valplant VlCodigoNavigation { get; set; } = null!;
}
