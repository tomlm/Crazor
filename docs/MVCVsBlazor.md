MVC

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# MVC Templates vs Blazor Templates

There are 2 crazor packages

* **Crazor.MVC** - implements ICardView using .cshtml files and ASP.NET MVC semantics (TagHelpers)
* **Crazor.Blazor** - implements ICardView using .razor files and Blazor semantics (Blazor Components)

## Differences

The 2 templating systems work almost the same, but there are some sublte differences between them.  

| Feature                 | MVC                | Blazor           | Comments                                                     |
| ----------------------- | ------------------ | ---------------- | ------------------------------------------------------------ |
| Routing                 | [CardRoute(...)]   | [CardRoute(...)] | *identical*                                                  |
| code behind             | @functions { ... } | @code { ... }    | MVC only supports @**functions**, Blazor supports both @**code** and @**functions** |
| Data-Binding attributes | [BindProperty]     | [Parameter]      |                                                              |
| Action handlers         | Verb="OnFoo"       | Verb = "OnFoo"   | *identical*                                                  |
| Route binding           | [FromCardRoute]    | [FromCardRoute]  | *identical*                                                  |
| Query Binding           | [FromCardQuery]    | [FromCardQuery]  | *identical*                                                  |
| Validation Attributes   | [Required]         | [Required]       | *identical*                                                  |

## Why use one over the other?

It really comes down to familiarity:

* If you are used to using .cshtml style you can continue to use it.
* If you like the blazor style you can use that as well

If you are building a portal experience that you want to consume cards in, then Blazor has the added advantage that it can just natively integrate right into your portal with minimal extra work.  
