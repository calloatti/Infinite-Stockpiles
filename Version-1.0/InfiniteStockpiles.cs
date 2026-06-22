using Bindito.Core;
using System;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Stockpiles;
using Timberborn.TemplateInstantiation;

namespace Calloatti.InfiniteStockpiles
{
  [Context("Game")]
  public class InfiniteStockpileConfigurator : Configurator
  {
    protected override void Configure()
    {
      Bind<InfiniteStockpileBehavior>().AsTransient();
      MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
      TemplateModule.Builder builder = new TemplateModule.Builder();
      builder.AddDecorator<StockpileSpec, InfiniteStockpileBehavior>();
      return builder.Build();
    }
  }

  public class InfiniteStockpileBehavior : BaseComponent, IAwakableComponent, IFinishedStateListener
  {
    private Stockpile _stockpile;
    private Inventory _inventory;
    private SingleGoodAllower _allower;
    private bool _isResetting;

    public void Awake()
    {
      _stockpile = GetComponent<Stockpile>();
      _allower = GetComponent<SingleGoodAllower>();
    }

    public void OnEnterFinishedState()
    {
      // This specifically targets the player-facing stockpile inventory, 
      // leaving the refund/construction inventories completely untouched.
      _inventory = _stockpile.Inventory;

      if (_inventory != null)
      {
        _inventory.InventoryStockChanged += OnInventoryStockChanged;
      }
      
      if (_allower != null)
      {
        _allower.DisallowedGoodsChanged += OnDisallowedGoodsChanged;
      }

      if (_inventory != null)
      {
        ResetToFiftyPercent();
      }
    }

    public void OnExitFinishedState()
    {
      if (_inventory != null)
      {
        _inventory.InventoryStockChanged -= OnInventoryStockChanged;
      }
      
      if (_allower != null)
      {
        _allower.DisallowedGoodsChanged -= OnDisallowedGoodsChanged;
      }
    }

    private void OnInventoryStockChanged(object sender, InventoryAmountChangedEventArgs e)
    {
      ResetToFiftyPercent();
    }

    private void OnDisallowedGoodsChanged(object sender, DisallowedGoodsChangedEventArgs e)
    {
      ResetToFiftyPercent();
    }

    private void ResetToFiftyPercent()
    {
      if (_isResetting || _inventory == null) return;
      _isResetting = true;

      try
      {
        string goodId = (_allower != null && _allower.HasAllowedGood) ? _allower.AllowedGood : null;
        
        // 1. Delete old/unwanted goods from THIS specific inventory when the filter changes
        foreach (GoodAmount stock in _inventory.Stock.ToList())
        {
          if (stock.GoodId != goodId)
          {
            int unreserved = _inventory.UnreservedAmountInStock(stock.GoodId);
            if (unreserved > 0)
            {
              _inventory.Take(new GoodAmount(stock.GoodId, unreserved));
            }
          }
        }

        // 2. Adjust the newly allowed good to exactly 50%
        if (!string.IsNullOrEmpty(goodId))
        {
          int halfCapacity = _inventory.Capacity / 2;
          if (halfCapacity > 0)
          {
            int currentAmount = _inventory.AmountInStock(goodId);

            if (currentAmount < halfCapacity)
            {
              int amountToAdd = halfCapacity - currentAmount;
              _inventory.GiveIgnoringCapacity(new GoodAmount(goodId, amountToAdd));
            }
            else if (currentAmount > halfCapacity)
            {
              int excess = currentAmount - halfCapacity;
              int unreserved = _inventory.UnreservedAmountInStock(goodId);
              int amountToTake = Math.Min(excess, unreserved);

              if (amountToTake > 0)
              {
                _inventory.Take(new GoodAmount(goodId, amountToTake));
              }
            }
          }
        }
      }
      finally
      {
        _isResetting = false;
      }
    }
  }
}