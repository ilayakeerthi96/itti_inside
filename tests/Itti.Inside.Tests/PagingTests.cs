using Itti.Inside.Application.Common;

namespace Itti.Inside.Tests;

public sealed class PagingTests
{
    [Fact]
    public void Middle_page_has_both_neighbours()
    {
        var page = new PagedResult<int>([3, 4], 2, 2, 5);

        Assert.True(page.HasPrevious);
        Assert.True(page.HasNext);
        Assert.Equal(3, page.TotalPages);
    }

    [Theory]
    [InlineData(-3, 1)]
    [InlineData(0, 1)]
    [InlineData(2, 2)]
    [InlineData(99, 3)]
    public void Out_of_range_pages_fall_back_to_a_valid_page(int requested, int expected)
    {
        Assert.Equal(expected, Paging.ClampPage(requested, pageSize: 2, totalCount: 5));
    }

    [Fact]
    public void Empty_list_is_a_single_empty_page()
    {
        var page = new PagedResult<int>([], Paging.ClampPage(1, pageSize: 2, totalCount: 0), 2, 0);

        Assert.Empty(page.Items);
        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.TotalPages);
        Assert.False(page.HasNext);
    }
}
