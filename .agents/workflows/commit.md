---
description: Commit changes using Conventional Commits format
---

# Commit Code (Conventional Commits)

## Step 1: Kiểm tra trước khi commit
// turbo
1. Build to verify no errors:
```
dotnet build FishCash/FishCash.csproj -f net9.0-windows10.0.19041.0
```

## Step 2: Xem changes
// turbo
2. Review staged and unstaged changes:
```
git status
```

// turbo
3. Review diff of changes:
```
git diff
```

## Step 3: Stage files
4. Stage relevant files (logical grouping):
```
git add <files>
```

## Step 4: Commit với Conventional Commits
5. Commit with a conventional commit message:
```
git commit -m "<type>(<scope>): <description>"
```

### Commit Types:
| Type | Khi nào dùng |
|------|-------------|
| `feat` | Thêm tính năng mới |
| `fix` | Sửa bug |
| `refactor` | Refactor code (không thêm feature, không fix bug) |
| `style` | Thay đổi UI/CSS/XAML styles |
| `docs` | Thay đổi tài liệu |
| `chore` | Cập nhật packages, config, build |
| `test` | Thêm hoặc sửa tests |

### Scopes phổ biến:
`models`, `services`, `viewmodels`, `views`, `data`, `navigation`, `print`, `payment`

### Ví dụ:
```
git commit -m "feat(models): add Transaction entity for income/expense tracking"
git commit -m "fix(services): handle null reference in CartService total calculation"
git commit -m "style(views): update PosPage layout for better touch targets"
```

## Lưu ý
- Sử dụng skill `git-commit` để tự động phân tích diff và tạo commit message
- Mỗi commit nên chỉ chứa **1 thay đổi logic** (không mix feature + fix)
- Description viết bằng **tiếng Anh**, dạng imperative ("add", "fix", "update", không phải "added", "fixed")
