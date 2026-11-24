using Docker.DotNet.Models;
using DockerController.Docker.Mapping;

namespace DockerController.Core.Tests;

public class DockerModelMapperTests
{
    private static ContainerStatsResponse StatsResponse(
        ulong totalUsage,
        ulong systemUsage,
        ulong previousTotalUsage,
        ulong previousSystemUsage,
        uint onlineCpus) =>
        new()
        {
            CPUStats = new CPUStats
            {
                CPUUsage = new CPUUsage { TotalUsage = totalUsage },
                SystemUsage = systemUsage,
                OnlineCPUs = onlineCpus,
            },
            PreCPUStats = new CPUStats
            {
                CPUUsage = new CPUUsage { TotalUsage = previousTotalUsage },
                SystemUsage = previousSystemUsage,
            },
            MemoryStats = new MemoryStats(),
        };

    [Fact]
    public void Cpu_percent_is_container_share_of_system_delta_times_cpu_count()
    {
        var response = StatsResponse(
            totalUsage: 150,
            systemUsage: 2000,
            previousTotalUsage: 100,
            previousSystemUsage: 1000,
            onlineCpus: 2);

        var stats = DockerModelMapper.ToContainerStats(response);

        Assert.Equal(10, stats.CpuPercent, precision: 6);
    }

    [Fact]
    public void Cpu_percent_is_zero_when_system_usage_has_not_changed()
    {
        var response = StatsResponse(
            totalUsage: 150,
            systemUsage: 1000,
            previousTotalUsage: 100,
            previousSystemUsage: 1000,
            onlineCpus: 2);

        var stats = DockerModelMapper.ToContainerStats(response);

        Assert.Equal(0, stats.CpuPercent);
    }

    [Fact]
    public void Cpu_percent_is_zero_when_previous_sample_is_missing()
    {
        var response = StatsResponse(
            totalUsage: 150,
            systemUsage: 2000,
            previousTotalUsage: 100,
            previousSystemUsage: 1000,
            onlineCpus: 2);
        response.PreCPUStats = null;

        var stats = DockerModelMapper.ToContainerStats(response);

        Assert.Equal(0, stats.CpuPercent);
    }

    [Fact]
    public void Memory_usage_and_limit_are_taken_from_memory_stats()
    {
        var response = StatsResponse(0, 0, 0, 0, 1);
        response.MemoryStats = new MemoryStats { Usage = 11_522_048, Limit = 2_147_483_648 };

        var stats = DockerModelMapper.ToContainerStats(response);

        Assert.Equal(11_522_048UL, stats.MemoryUsageBytes);
        Assert.Equal(2_147_483_648UL, stats.MemoryLimitBytes);
    }
}
