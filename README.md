# Pontello Import

Pontello Import is a Niagara-based, family-owned import business founded in 2025. Specialize in supplying high-quality, hard-to-find karting and motorsports components to customers across Canada and the United States.

# Project Setup & Contribution Guide
## Getting Started (GitHub Workflow)
### Setup
1. Clone the repository.
2. Open the solution in **Visual Studio**.
3. Open the **Package Manager Console**.
4. Create a new branch using your name:
   ```bash
   git checkout -b yourName
	```
5. Write and test your code.

## Pushing Changes to GitHub
### Steps
```bash
git add .
```
```bash
git commit -m "your message"
```
### Before Pushing Your Changes
 Pull the latest changes from the dev branch:
```bash
git pull origin dev
```
```bash
git push OR git push --set-upstream origin yourName
```
Create a pull request by matching your branch to the ""dev"" branch on github website.
Notify the team you have created a pull request for review.

--------------------------------------------------------------------------------------------------------
# Database Common commands

### To add a new migration:
```bash
Add-Migration -Context PontelloDbContext -OutputDir Data\MMigrations Initial
```
### To update database:
```bash
Update-Database -Context PontelloDbContext
```


Use the package manager [pip](https://pip.pypa.io/en/stable/) to install foobar.

```bash
pip install foobar
```
