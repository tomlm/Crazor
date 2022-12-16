
# Components

## Using tag helper
```html
    <vc:person name="Matt Hidinger" url="https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg"
               size="@AdaptiveImageSize.Small" text="AdaptiveCard guru extraordinaire." />
```

## Using code

```c#
        @Person(name, url, size) 
        => 
        @await Component.InvokeAsync("Person", new PersonViewModel
        {
            Name = "Tom Laird-McConnell",
            Url = "https://avatars.githubusercontent.com/u/17789481?s=96&v=4",
            Size = AdaptiveImageSize.Medium.ToString()
        })
```

## Component issues
1. Can't nest components.  WTF?
