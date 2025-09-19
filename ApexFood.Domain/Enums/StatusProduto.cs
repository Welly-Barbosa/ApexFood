// ApexFood.Domain/Enums/StatusProduto.cs
namespace ApexFood.Domain.Enums;

/// <summary>
/// Define os diferentes estágios do ciclo de vida de um ProdutoModelo.
/// </summary>
public enum StatusProduto
{
    /// <summary>
    /// O produto base foi criado, mas sua ficha técnica (receita) ainda não foi definida.
    /// </summary>
    PendenteFichaTecnica = 1,

    /// <summary>
    /// A ficha técnica foi criada e o CMV pode ser calculado, mas o preço de venda final não foi definido.
    /// </summary>
    PendentePreco = 2,

    /// <summary>
    /// O produto possui ficha técnica e preço de venda, estando pronto para ser vendido ou incluído em promoções.
    /// </summary>
    Completo = 3
}