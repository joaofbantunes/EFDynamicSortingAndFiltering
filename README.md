# EFDynamicSortingAndFiltering

Playing around with expression generation for query filtering and sorting with Entity Framework Core.

This is definitely over engineered, and most likely not the best approach the majority of times, but something similar to the `HardcodedPropertyTypeInferringFilteringStrategy` and `HardcodedPropertyTypeInferringSortingStrategy` strategies was useful in a past project to ease the development of a bunch of pages with grids.

I added here `ReflectionBasedPropertyTypeInferringFilteringStrategy` and `ReflectionBasedPropertyTypeInferringSortingStrategy` to see how much more generic the original code could be made, and what impact would it have on performance.

## Using it

### Filtering

The usual:
```csharp
await _ctx.SampleEntities.Where(e => e.Id == 2).ToListAsync();
```

Translates into:
```csharp
await _ctx.SampleEntities.Filter(new Filter { Type = FilterType.Equals, PropertyName = nameof(SampleEntity.Id), Values = new[] { "2" } }).ToListAsync();
```

It's much more verbose, but the goal is to create the `Filter` based on string based info (like info got in a AJAX request).

### Sorting

The usual:
```csharp
await _ctx.SampleEntities.OrderByDescending(e => e.SomeNullableInt).ToListAsync();
```

Translates into:
```csharp
await _ctx.SampleEntities.Sort(new SortCriteria { PropertyName = nameof(SampleEntity.SomeNullableInt), Direction = SortDirection.Descending }).ToListAsync();
```

Like in the filtering case,  it's much more verbose, but the goal is to create the `SortCriteria` based on string based info (like info got in a AJAX request).

## Testing

Running a docker container with postgres for tests
docker run -p 5432:5432 --restart unless-stopped --name postgres -e POSTGRES_USER=user -e POSTGRES_PASSWORD=pass -d postgres

## Got better ideas?

This was useful for me in the past, but I'm sure there are better ways, or at least ways to improve this, so if you've got any suggestion, please share!