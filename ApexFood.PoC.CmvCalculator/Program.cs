using System.Diagnostics;

// --- 1. ESTRUTURAS DE DADOS MÍNIMAS (MOCKS) ---

public abstract record CustoItem(Guid Id, string Nome);
public record Insumo(Guid Id, string Nome, decimal Preco) : CustoItem(Id, Nome);
public record Produto(Guid Id, string Nome, List<ReceitaItem> Receita) : CustoItem(Id, Nome);
public record ReceitaItem(Guid ItemId, decimal Quantidade);

// --- 2. O SERVIÇO DE CÁLCULO (O ALGORITMO) ---

public class CmvCalculatorService
{
    private readonly Dictionary<Guid, CustoItem> _itensMapa;

    public CmvCalculatorService(IEnumerable<CustoItem> todosOsItens)
    {
        _itensMapa = todosOsItens.ToDictionary(item => item.Id);
    }

    public decimal CalcularCmv(Guid produtoId)
    {
        if (!_itensMapa.TryGetValue(produtoId, out var item))
        {
            return 0;
        }

        if (item is Insumo insumo)
        {
            return insumo.Preco;
        }

        if (item is Produto produto)
        {
            decimal custoTotalReceita = 0;
            foreach (var receitaItem in produto.Receita)
            {
                custoTotalReceita += receitaItem.Quantidade * CalcularCmv(receitaItem.ItemId);
            }
            return custoTotalReceita;
        }

        return 0;
    }
}

// --- 3. O CENÁRIO DE TESTE (A EXECUÇÃO DA PoC com 3 NÍVEIS) ---

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Iniciando PoC de Cálculo de CMV com 3 níveis...");

        // --- PREPARAÇÃO (ARRANGE) ---

        // Insumos com seus preços (Nível 4)
        var farinha = new Insumo(Guid.NewGuid(), "Farinha de Trigo", 10.0m);
        var chocolatePo = new Insumo(Guid.NewGuid(), "Chocolate em Pó 50%", 20.0m);
        var ovo = new Insumo(Guid.NewGuid(), "Ovo (unidade)", 0.50m);
        var leite = new Insumo(Guid.NewGuid(), "Leite (litro)", 5.0m);
        var acucar = new Insumo(Guid.NewGuid(), "Açúcar", 8.0m);

        // ==================================================================
        // NOVA SUB-RECEITA DE NÍVEL 3
        // ==================================================================
        var misturaSeca = new Produto(Guid.NewGuid(), "Mistura Seca (Sub-receita Nv. 3)", new List<ReceitaItem>
        {
            new(farinha.Id, 0.5m),      // 0.5 kg de farinha
            new(chocolatePo.Id, 0.2m), // 0.2 kg de chocolate
            new(acucar.Id, 0.3m)       // 0.3 kg de açúcar
        });

        // ==================================================================
        // SUB-RECEITA DE NÍVEL 2 ATUALIZADA (agora usa a sub-receita de Nível 3)
        // ==================================================================
        var massaChocolate = new Produto(Guid.NewGuid(), "Massa de Chocolate (Sub-receita Nv. 2)", new List<ReceitaItem>
        {
            new(misturaSeca.Id, 1),    // 1 unidade da "Mistura Seca"
            new(ovo.Id, 2),            // 2 ovos
            new(leite.Id, 0.25m)       // 0.25 L de leite
        });

        // Produto Final (Nível 1)
        var boloChocolate = new Produto(Guid.NewGuid(), "Bolo de Chocolate (Produto Final Nv. 1)", new List<ReceitaItem>
        {
            new(massaChocolate.Id, 1), // 1 unidade da "Massa de Chocolate"
            new(ovo.Id, 1)             // Mais 1 ovo para a cobertura (exemplo)
        });

        // Adicionamos a nova sub-receita à lista de todos os itens conhecidos
        var todosOsItens = new List<CustoItem> { farinha, chocolatePo, ovo, leite, acucar, misturaSeca, massaChocolate, boloChocolate };
        var calculator = new CmvCalculatorService(todosOsItens);

        // --- AÇÃO E MEDIÇÃO (ACT) ---

        var stopwatch = new Stopwatch();
        decimal cmvFinal = 0;
        int numeroDeCalculos = 50;

        stopwatch.Start();
        for (int i = 0; i < numeroDeCalculos; i++)
        {
            cmvFinal = calculator.CalcularCmv(boloChocolate.Id);
        }
        stopwatch.Stop();

        // --- VERIFICAÇÃO (ASSERT) ---
        Console.WriteLine("\n--- Resultados ---");
        Console.WriteLine($"CMV calculado para '{boloChocolate.Nome}': {cmvFinal:C}");
        Console.WriteLine($"Tempo total para calcular {numeroDeCalculos} produtos: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Tempo médio por produto: {(double)stopwatch.ElapsedMilliseconds / numeroDeCalculos:F2} ms");

        // Verificação manual do cálculo:
        // Custo Mistura Seca = (0.5*10) + (0.2*20) + (0.3*8) = 5 + 4 + 2.4 = 11.40
        // Custo Massa = (1 * 11.40) + (2*0.5) + (0.25*5) = 11.40 + 1 + 1.25 = 13.65
        // Custo Bolo = (1 * 13.65) + (1 * 0.5) = 14.15
        Console.WriteLine($"Cálculo esperado: {14.15:C}");
        Console.WriteLine("------------------");
    }
}