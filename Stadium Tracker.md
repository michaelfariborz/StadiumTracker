Sports Stadium Tracker Application

This will be a .NET 10 Blazor Server application.  It will be used to track which Sports Stadiums the user has been to and which ones they have not.

Use Microsoft SQL Server as the database, I have that installed locally.

Stadiums will be broken into multiple leagues:
- Major League Baseball (MLB)
- National Basketball Association (NBA)
- National Football League (NFL)
- Major League Soccer (MLS)
- AAA Minor League Baseball (AAA)
- AA Minor League Baseball (AA)
- A Minor League Baseball (A)

The UI should allow the user to sign in.  It should display a map that show pins for all of the Stadiums in the Stadiums table for the selected league.  The stadiums that the user has visited should show with a green pin.  The stadiums that the user has not visited yet, should show a clear pin.

When the user clicks on a pin that they have visited, it should show the Stadium Information, the home team, the date(s) that they have attended the game there (optional), the teams that played (optional) and the score of the game (optional).  That dialog should also let the user add a new visit, where they can specify all of the information and save it to the database.

There should also be a separate page that shows the same information in a tabular form.  The user will be able to add new visits there as well.  Also, they should be able to edit or remove any visits.

There should also be a set of Admin screens that: 
- Allows the user to see all of the leagues that are currently in the table, where they can add new leagues.  
- Allows the user to see all the stadiums, and enter a new one.

There should also be the standard user management screens.
