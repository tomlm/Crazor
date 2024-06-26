



![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Custom Card View

Any class can implement **ICardView**, but typically all you want to do is have an alternate way of defining the rendering of the view with all of the same functionality of other CardView implementations. (validation, action reflection, data binding etc.)
To do this you can simply derive from **CustomCardView** and override **RenderViewAsync**.

## Example
In this example we are using the object model to create a card using C# code to bind our values into the adaptive card.

```C#
namespace Example
{
    public class MyCodeView : CustomCardView
    {
        public int Counter { get; set; }

        public override async Task<AdaptiveCard?> RenderViewAsync(bool isPreview, CancellationToken cancellationToken)
        {
            return new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>() 
                { 
                    new AdaptiveTextBlock($"Counter is {this.Counter}") 
                },
                Actions = new List<AdaptiveAction>() 
                {
                    new AdaptiveExecuteAction()
                    { 
                        Verb = nameof(OnIncrement), 
                        Title = "Increment"
                    }
                }
            };
        }

        public void OnIncrement()
            => this.Counter++;
    }
}
```



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
