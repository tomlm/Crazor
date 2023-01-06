

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)


# TagHelpers
TagHelpers allow you to bundle up a bunch of complex adaptive card layout and replace it with a new tag, esssentially creating a reusable template.

Tag helpers are defined the TagHelper folder by

Creating a RazorTagHelper
![image](https://user-images.githubusercontent.com/17789481/190312173-244ac183-195a-4030-a1ea-713697c1030e.png)

* Create a class derived from **RazorTagHelper**
* Add properties for authoring, and add **HtmlAttribute** attribute to them.  This defines the "input" of your tag helper.
* Create a **default.cshtml** with **@model *YourTagHelperClassName* ** 
* Define your template data bindings.

> 



![image](https://user-images.githubusercontent.com/17789481/197365048-6a74c3d5-85cd-4c04-a07a-eef2a46e0ddf.png)
