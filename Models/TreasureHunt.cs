using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class TreasureMap
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Rows { get; set; }

    [Required]
    public int Columns { get; set; }

    [Required]
    public int MaxTreasureValue { get; set; }

    [Required]
    public string Matrix { get; set; }

    public double? MinFuel { get; set; }
}

public class MatrixInput
{
    [Required]
    public int Rows { get; set; }

    [Required]
    public int Columns { get; set; }

    [Required]
    public int MaxTreasureValue { get; set; }

    [Required]
    public int[,] Matrix { get; set; }
}
public class MatrixInputDto
{
    [Required]
    public int Rows { get; set; }

    [Required]
    public int Columns { get; set; }

    [Required]
    public int MaxTreasureValue { get; set; }

    [Required]
    public string Matrix { get; set; }
}